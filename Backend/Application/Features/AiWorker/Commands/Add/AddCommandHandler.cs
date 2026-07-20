using Application.Common.Interfaces.Services;
using Application.Features.AiWorker.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.AiWorker.Commands.Add;

public class AddCommandHandler : IRequestHandler<AddCommand, AddResultDto>
{
    private readonly IAiWorkerClient _aiWorker;
    private readonly ILogger<AddCommandHandler> _logger;

    public AddCommandHandler(IAiWorkerClient aiWorker, ILogger<AddCommandHandler> logger)
    {
        _aiWorker = aiWorker;
        _logger = logger;
    }

    public async Task<AddResultDto> Handle(AddCommand request, CancellationToken cancellationToken)
    {
        // Round-trips through the gRPC AiWorker service (see AiWorkerGrpcClient).
        var sum = await _aiWorker.AddAsync(request.A, request.B, cancellationToken);
        _logger.LogInformation("AiWorker gRPC Add({A}, {B}) = {Sum}", request.A, request.B, sum);

        return new AddResultDto { A = request.A, B = request.B, Sum = sum };
    }
}
