/*
It runs the AGENTIC LOOP:
1. Get Tools from MCP server
2. Send Query + Tools to LLM (e.g. Gemini/Claude)
3. If LLM says "call this tool" → call it via MCP
4. Feed result back to LLM
5. Repeat until LLM gives a plain text final answer
*/
using Application.Common.Interfaces.Services;
using Application.Features.Chat.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Application.Features.Chat.Commands;

public class SendChatCommandHandler : IRequestHandler<SendChatCommand, ChatResponseDto>
{
    private readonly IMcpService _mcpService;
    private readonly ILlmFactory _llmFactory;
    private readonly ILogger<SendChatCommandHandler> _logger;
    public SendChatCommandHandler(
                IMcpService mcpService,
                ILlmFactory llmFactory,
                ILogger<SendChatCommandHandler> logger)
    {
        _mcpService = mcpService;
        _llmFactory = llmFactory;
        _logger = logger;        
    }

    public async Task<ChatResponseDto> Handle(SendChatCommand request, CancellationToken ct)
    {
        _logger.LogInformation($"Handling SendChatCommand. Provider={request.Provider} Query={request.Query}");

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

        // Step 4: Start conversation with llm
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, request.Query)
        };
        var toolCallLog = new List<ToolCallRecord>();

        // This is the AGENTIC LOOP
        while (true)
        {
            var response = await llm.GetResponseAsync(messages, chatOptions, ct);

            // add llm response to conversation history
            foreach (var message in response.Messages)
            {
                messages.Add(message);
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
                                .SelectMany(txt => txt.Text)
                                .ToList());
                _logger.LogInformation($"Chat complete. ToolsUsed={toolCallLog.Count}");
                return new ChatResponseDto
                {
                    Answer = answer,
                    Provider = request.Provider,
                    ToolCalls = toolCallLog
                };
            }
            // Execute tool calls and collect results
            var toolResultMessages = new List<ChatMessage>();
            foreach (var toolCall in toolCalls)
            {
                _logger.LogInformation($"LLM called tool: {toolCall.Name} with args: {toolCall.Arguments}");
                var args = toolCall.Arguments is not null
                    ? new Dictionary<string, object?>(toolCall.Arguments)
                    : new Dictionary<string, object?>();

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

                // Wrap result in the format the LLM expects for the next turn
                toolResultMessages.Add(new ChatMessage(
                    ChatRole.Tool,
                    contents: [new FunctionResultContent(toolCall.CallId ?? toolCall.Name, result)]
                ));
            }
            messages.AddRange(toolResultMessages);
        }
    }
}
