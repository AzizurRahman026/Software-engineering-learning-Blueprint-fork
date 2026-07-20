using Application.Common.Interfaces.Services;
using Contracts.Grpc;
using Grpc.Net.Client;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Grpc;

public class AiWorkerGrpcClient : IAiWorkerClient, IDisposable
{
    // One channel for the app's lifetime: a channel owns the HTTP/2 connection and
    // is expensive to create, so this class is registered as a singleton.
    private readonly GrpcChannel _channel;
    private readonly Calculator.CalculatorClient _client;

    public AiWorkerGrpcClient(IOptions<AiWorkerOptions> options)
    {
        _channel = GrpcChannel.ForAddress(options.Value.Endpoint);
        _client = new Calculator.CalculatorClient(_channel);
    }

    public async Task<int> AddAsync(int a, int b, CancellationToken cancellationToken = default)
    {
        var reply = await _client.AddAsync(
            new AddRequest { A = a, B = b },
            deadline: DateTime.UtcNow.AddSeconds(5),
            cancellationToken: cancellationToken);

        return reply.Sum;
    }

    public void Dispose() => _channel.Dispose();
}
