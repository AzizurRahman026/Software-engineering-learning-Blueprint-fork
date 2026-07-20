using Application.Features.AiWorker.DTOs;
using MediatR;

namespace Application.Features.AiWorker.Commands.Add;

/// <summary>Sends two integers to the AiWorker gRPC service to be summed.</summary>
public class AddCommand : IRequest<AddResultDto>
{
    public int A { get; set; }
    public int B { get; set; }
}
