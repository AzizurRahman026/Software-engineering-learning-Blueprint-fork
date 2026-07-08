using Application.Common.Interfaces.Services;
using Application.Features.Chat.Commands.SuggestAndSaveThreadTitle;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Application.Features.Chat;

/// <summary>
/// Unit tests for the structured-output slice — the "mocking LLM clients"
/// testing-cadence task. No network, no MongoDB: a hand-rolled IChatClient returns a parses. 
/// 
/// Pins four behaviors: 
/// (1) valid model JSON is validated then PERSISTED via UpdateThreadTitleAsync; 
/// (2) malformed model output throws LlmUnavailableException (the 502 path) and persists NOTHING; 
/// (3) post-parse business bounds hold (80-char title cap, topics lowercased/deduped/capped at 3); 
/// (4) a thread with no user message is NotFound and the LLM is never billed.
/// </summary>
public class SuggestAndSaveThreadTitleCommandHandlerTests
{
    // ---------- Test doubles (interface-level, Domain + Application only) ----------

    private sealed class SpyChatHistoryStore : IChatHistoryStore
    {
        public List<ChatMessage> History { get; init; } = [];
        public List<(string ThreadId, string UserId, string Title)> TitleUpdates { get; } = [];

        public Task<List<ChatMessage>> GetHistoryAsync(string threadId) => Task.FromResult(History);
        public Task SaveChatMessageAsync(string threadId, List<ChatMessage> messages) => Task.CompletedTask;
        public Task<string> CreateThreadAsync(string userId) => Task.FromResult("t-new");
        public Task DeleteThreadAsync(string threadId, string userId) => Task.CompletedTask;
        public Task<List<ChatThreadInfo>> GetAllThreadAsync(string userId) => Task.FromResult(new List<ChatThreadInfo>());

        public Task UpdateThreadTitleAsync(string threadId, string userId, string title)
        {
            TitleUpdates.Add((threadId, userId, title));
            return Task.CompletedTask;
        }
    }

    /// <summary>Returns a fixed assistant reply; records whether it was ever called.</summary>
    private sealed class FakeChatClient(string reply) : IChatClient
    {
        public bool WasCalled { get; private set; }

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, reply)));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException("Handler under test never streams.");

        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    private sealed class FakeLlmFactory(IChatClient client) : ILlmFactory
    {
        public IChatClient Create(LlmProvider provider) => client;
    }

    // ---------- Helpers ----------

    private static SuggestAndSaveThreadTitleCommandHandler BuildHandler(SpyChatHistoryStore store, IChatClient llm)
        => new(store, new FakeLlmFactory(llm),
               NullLogger<SuggestAndSaveThreadTitleCommandHandler>.Instance);

    private static SpyChatHistoryStore StoreWithUserMessage(string text = "How do MongoDB compound indexes work?")
        => new() { History = [new ChatMessage(ChatRole.User, text)] };

    private static SuggestAndSaveThreadTitleCommand Command() => new()
    {
        ThreadId = "thread-1",
        UserId = "user-1",
        Provider = LlmProvider.Gemini,
    };

    // ---------- Tests ----------

    [Fact]
    public async Task ValidModelOutput_ReturnsTitle_AndPersistsItForTheOwner()
    {
        var store = StoreWithUserMessage();
        var handler = BuildHandler(store, new FakeChatClient($$"""{"title":"MongoDB Index Basics","topics":["mongodb", "indexes"]}"""));

        var result = await handler.Handle(Command(), CancellationToken.None);

        Assert.Equal("MongoDB Index Basics", result.Title);
        Assert.Equal(["mongodb", "indexes"], result.Topics);

        // The write must go through the store with the caller's identity — ownership
        // enforcement happens inside the store ($set scoped by Id && UserId, ADR 0001).
        var update = Assert.Single(store.TitleUpdates);
        Assert.Equal(("thread-1", "user-1", "MongoDB Index Basics"), update);
    }

    [Fact]
    public async Task MalformedModelOutput_ThrowsLlmUnavailable_AndPersistsNothing()
    {
        var store = StoreWithUserMessage();
        // Prose instead of JSON — the classic "provider model update" drift Day 24 warned about.
        var handler = BuildHandler(store, new FakeChatClient($$"""Sure! Here is a nice title for you."""));

        await Assert.ThrowsAsync<LlmUnavailableException>(
            () => handler.Handle(Command(), CancellationToken.None));

        Assert.Empty(store.TitleUpdates); // a failed suggestion must never write
    }

    [Fact]
    public async Task OverlongTitle_IsCappedAt80Chars_WithEllipsis_AndTheCappedValueIsPersisted()
    {
        var store = StoreWithUserMessage();
        var longTitle = new string('a', 100);
        var handler = BuildHandler(store, new FakeChatClient($$"""{"title":"{{longTitle}}","topics":[]}"""));

        var result = await handler.Handle(Command(), CancellationToken.None);

        Assert.Equal(81, result.Title.Length); // 80 kept chars + ellipsis
        Assert.EndsWith("…", result.Title);
        Assert.Equal(result.Title, Assert.Single(store.TitleUpdates).Title); // returned == persisted
    }

    [Fact]
    public async Task Topics_AreTrimmed_Lowercased_Deduped_AndCappedAtThree()
    {
        var store = StoreWithUserMessage();
        var handler = BuildHandler(store, new FakeChatClient(
            $$"""{"title":"Indexing Deep Dive","topics":[" MongoDB ","mongodb","INDEXES","","performance","extra"]}"""));

        var result = await handler.Handle(Command(), CancellationToken.None);

        Assert.Equal(["mongodb", "indexes", "performance"], result.Topics);
    }

    [Fact]
    public async Task ThreadWithoutUserMessage_ThrowsNotFound_AndNeverCallsTheLlm()
    {
        var store = new SpyChatHistoryStore
        {
            History = [new ChatMessage(ChatRole.Assistant, "orphaned assistant text")]
        };
        var llm = new FakeChatClient($$"""{"title":"unused","topics":[]}""");
        var handler = BuildHandler(store, llm);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(Command(), CancellationToken.None));

        Assert.False(llm.WasCalled);      // no user content → no billable call
        Assert.Empty(store.TitleUpdates);
    }
}
