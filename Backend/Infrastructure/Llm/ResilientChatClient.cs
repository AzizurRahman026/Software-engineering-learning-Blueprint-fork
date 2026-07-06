using System.Net;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Llm;

/// <summary>
/// Wraps a provider IChatClient with a per-attempt timeout and a bounded retry
/// (exponential backoff + jitter) for transient failures: network errors, 429
/// rate limits, and 5xx provider errors.
///
/// Placement matters: this sits INSIDE FunctionInvokingChatClient in the
/// pipeline, so each individual provider round-trip is retried — not the whole
/// multi-step tool-calling loop (which would re-execute tools).
///
/// Provider SDKs that throw their own exception types (instead of
/// HttpRequestException) get added to <see cref="IsTransient"/> as they are
/// observed in production logs.
/// </summary>
public sealed class ResilientChatClient : DelegatingChatClient
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(100);
    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(1);

    private readonly ILogger _logger;

    public ResilientChatClient(IChatClient innerClient, ILogger<ResilientChatClient> logger)
        : base(innerClient)
    {
        _logger = logger;
    }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; ; attempt++)
        {
            // Linked CTS: fires on caller cancellation OR per-attempt timeout.
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(AttemptTimeout);

            try
            {
                return await base.GetResponseAsync(messages, options, timeoutCts.Token);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The CALLER cancelled (user disconnected / request aborted) — never retry.
                throw;
            }
            catch (Exception ex) when (attempt < MaxAttempts && IsTransient(ex, timeoutCts))
            {
                var delay = BaseDelay * Math.Pow(2, attempt - 1)
                            + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 250));

                _logger.LogWarning(ex,
                    "Transient LLM failure on attempt {Attempt}/{MaxAttempts}; retrying in {DelayMs} ms",
                    attempt, MaxAttempts, (int)delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private static bool IsTransient(Exception ex, CancellationTokenSource timeoutCts) => ex switch
    {
        // Reaching here means the caller did NOT cancel (filtered above),
        // so an OCE can only be our per-attempt timeout firing.
        OperationCanceledException => timeoutCts.IsCancellationRequested,

        HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests } => true,   // 429
        HttpRequestException { StatusCode: >= HttpStatusCode.InternalServerError } => true, // 5xx
        HttpRequestException { StatusCode: null } => true, // DNS / socket / connection reset

        _ => false, // 400/401/403 etc. — retrying cannot fix these
    };
}
