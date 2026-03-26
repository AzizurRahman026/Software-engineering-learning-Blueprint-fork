
using Domain.Enums;
using Microsoft.Extensions.AI;

namespace Application.Common.Interfaces.Services;

public interface ILlmFactory
{
    IChatClient Create(LlmProvider provider);
}
