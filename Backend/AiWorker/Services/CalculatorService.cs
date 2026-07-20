using Contracts.Grpc;
using Grpc.Core;

namespace AiWorker.Services;

public class CalculatorService : Calculator.CalculatorBase
{
    private readonly ILogger<CalculatorService> _logger;

    public CalculatorService(ILogger<CalculatorService> logger)
    {
        _logger = logger;
    }

    public override Task<AddReply> Add(AddRequest request, ServerCallContext context)
    {
        var sum = request.A + request.B;
        _logger.LogInformation("gRPC Add({A}, {B}) = {Sum}", request.A, request.B, sum);
        return Task.FromResult(new AddReply
        { 
            Sum = sum
        });
    }
}
