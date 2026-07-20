namespace Application.Common.Interfaces.Services;

public interface IAiWorkerClient
{
    Task<int> AddAsync(int a, int b, CancellationToken cancellationToken = default);
}
