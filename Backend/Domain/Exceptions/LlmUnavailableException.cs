namespace Domain.Exceptions;

/// <summary>
/// Raised when an LLM provider call fails (network/auth/quota) or the agentic
/// loop exceeds its safety cap. Mapped to 502 ProblemDetails by GlobalExceptionMiddleware.
/// </summary>
public class LlmUnavailableException : Exception
{
    public LlmUnavailableException() { }
    public LlmUnavailableException(string message) : base(message) { }
    public LlmUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}
