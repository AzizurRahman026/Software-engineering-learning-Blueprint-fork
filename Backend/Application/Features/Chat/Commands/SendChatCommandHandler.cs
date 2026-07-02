/*
It runs the AGENTIC LOOP:
1. Get Tools from MCP server
2. Send Query + Tools to LLM (e.g. Gemini/Claude)
3. If LLM says "call this tool" → call it via MCP
4. Feed result back to the LLM
5. Repeat until the LLM gives a plain text final answer
*/
using Application.Common.Interfaces.Services;
using Application.Features.Chat.DTOs;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Features.Chat.Commands;

public class SendChatCommandHandler : IRequestHandler<SendChatCommand, ChatResponseDto>
{
    // Safety cap: an LLM that keeps requesting tools would otherwise loop (and bill) forever.
    private const int MaxToolIterations = 10;

    private readonly IMcpService _mcpService;
    private readonly ILlmFactory _llmFactory;
    private readonly IChatHistoryStore _historyStore;
    private readonly ILogger<SendChatCommandHandler> _logger;
    public SendChatCommandHandler(
                IMcpService mcpService,
                ILlmFactory llmFactory,
                IChatHistoryStore chatHistoryStore,
                ILogger<SendChatCommandHandler> logger)
    {
        _mcpService = mcpService;
        _llmFactory = llmFactory;
        _historyStore = chatHistoryStore;
        _logger = logger;
    }

    public async Task<ChatResponseDto> Handle(SendChatCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Handling SendChatCommand. Provider={Provider} QueryLength={QueryLength}",
            request.Provider, request.Query?.Length ?? 0);

        var threadId = request.ThreadId;
        if (string.IsNullOrWhiteSpace(threadId))
        {
            threadId = await _historyStore.CreateThreadAsync(request.UserId);
            _logger.LogInformation("Created new thread {ThreadId}", threadId);
        }

        var threadMessages = await _historyStore.GetHistoryAsync(threadId);

        // Step 1: Ask MCP server what tools it has
        var tools = await _mcpService.GetToolsAsync(ct);

        // step 2: create llm via factory
        var llm = _llmFactory.Create(request.Provider);

        // step 3: Tell the LLM of tools
        var chatOptions = new ChatOptions
        {
            Tools = [.. tools],
            ToolMode = ChatToolMode.Auto
        };

        // Step 4: Start conversation with llm by add user latest query
        threadMessages.Add(new ChatMessage(ChatRole.User, request.Query));
        var toolCallLog = new List<ToolCallRecord>();
        long totalInputTokens = 0, totalOutputTokens = 0;

        // This is the AGENTIC LOOP
        for (var iteration = 1; ; iteration++)
        {
            if (iteration > MaxToolIterations)
            {
                _logger.LogWarning("Agentic loop exceeded {MaxIterations} iterations. Provider={Provider} ToolCalls={ToolCallCount}",
                    MaxToolIterations, request.Provider, toolCallLog.Count);
                throw new LlmUnavailableException(
                    $"The AI agent exceeded the maximum of {MaxToolIterations} tool iterations without producing a final answer.");
            }

            ChatResponse response;

            try
            {
                response = await llm.GetResponseAsync(threadMessages, chatOptions, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LLM call failed. Provider={Provider} Iteration={Iteration}", request.Provider, iteration);
                throw new LlmUnavailableException($"The {request.Provider} provider failed to respond.", ex);
            }

            // Cost observability: accumulate token usage across every loop round-trip.
            if (response.Usage is { } usage)
            {
                totalInputTokens += usage.InputTokenCount ?? 0;
                totalOutputTokens += usage.OutputTokenCount ?? 0;
            }
            // add llm response to conversation history (Gemini MEAI mapper can mis-cast mixed TextContent + tool calls)
            foreach (var message in response.Messages)
            {
                threadMessages.Add(NormalizeAssistantMessageForHistory(message));
            }

            // did LLM call a tool?
            var toolCalls = response.Messages
                .SelectMany(m => m.Contents)
                .OfType<FunctionCallContent>()
                .ToList();

            // no tools called, response is final answer
            if (toolCalls.Count == 0)
            {
                var answer = string.Join("\n",
                    response.Messages
                        .SelectMany(m => m.Contents)
                        .OfType<TextContent>()
                        .Select(c => c.Text));
                _logger.LogInformation(
                    "Chat complete. Provider={Provider} ToolsUsed={ToolsUsed} InputTokens={InputTokens} OutputTokens={OutputTokens}",
                    request.Provider, toolCallLog.Count, totalInputTokens, totalOutputTokens);
                await _historyStore.SaveChatMessageAsync(threadId, threadMessages);

                return new ChatResponseDto
                {
                    ThreadId = threadId,
                    Answer = answer,
                    Provider = request.Provider,
                    ToolCalls = toolCallLog
                };
            }
            // Execute tool calls and collect results
            var toolResultMessages = new List<ChatMessage>();
            foreach (var toolCall in toolCalls)
            {
                _logger.LogInformation("LLM called tool {ToolName}", toolCall.Name);
                var args = ParseArguments(toolCall.Arguments);

                string result;
                try
                {
                    result = await _mcpService.CallToolAsync(toolCall.Name, args, ct);
                }
                catch (Exception ex)
                {
                    result = $"Tool error: {ex.Message}";
                    _logger.LogWarning("Tool {Tool} failed: {Error}", toolCall.Name, ex.Message);
                }
                toolCallLog.Add(ToolCallRecord.Create(toolCall.Name, result));

                var toolResultJson = JsonSerializer.SerializeToElement(new { result });

                toolResultMessages.Add(new ChatMessage(
                    ChatRole.Tool,
                    contents: [new FunctionResultContent(toolCall.CallId ?? toolCall.Name, toolResultJson)]
                ));
            }
            threadMessages.AddRange(toolResultMessages);
        }
    }

    /// <summary>
    /// Strips plain text from assistant turns that also request tool calls so the Gemini MEAI client
    /// does not hit InvalidCastException (TextContent treated as JsonElement) on the next round.
    /// </summary>
    private static ChatMessage NormalizeAssistantMessageForHistory(ChatMessage message)
    {
        if (message.Role != ChatRole.Assistant)
            return message;

        var functionCalls = message.Contents.OfType<FunctionCallContent>().ToList();
        if (functionCalls.Count == 0)
            return message;

        return new ChatMessage(ChatRole.Assistant, contents: [.. functionCalls]);
    }

    private static Dictionary<string, object?> ParseArguments(object? arguments)
    {
        if (arguments is IDictionary<string, object?> dict)
            return dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (arguments is JsonElement json)
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(json.GetRawText())!;

        if (arguments is string str)
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(str)!;

        return new();
    }
}
