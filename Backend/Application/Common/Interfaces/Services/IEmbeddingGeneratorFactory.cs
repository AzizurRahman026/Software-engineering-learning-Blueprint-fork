using Domain.Enums;
using Microsoft.Extensions.AI;

namespace Application.Common.Interfaces.Services;

/// <summary>
/// Gives us a tool that turns text into "embeddings" — the number-lists that capture a text's
/// MEANING (see <see cref="Application.Common.Ai.VectorMath"/>), for the AI provider we pick.
///
/// It's the meaning-maker's counterpart to <see cref="ILlmFactory"/> (which gives us the chat AI):
/// chat and embeddings are kept separate because they're different AI models that change over time
/// for different reasons. This is just the "plug" — the actual provider hookup lives in Infrastructure.
/// </summary>
public interface IEmbeddingGeneratorFactory
{
    /// <summary>Returns a ready-to-use, cached embedding tool for the chosen <paramref name="provider"/>.</summary>
    IEmbeddingGenerator<string, Embedding<float>> Create(LlmProvider provider);
}
