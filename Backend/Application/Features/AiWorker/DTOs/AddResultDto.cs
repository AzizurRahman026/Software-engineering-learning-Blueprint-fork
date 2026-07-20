namespace Application.Features.AiWorker.DTOs;

/// <summary>Result of the AiWorker gRPC Add call: the operands and the computed sum.</summary>
public class AddResultDto
{
    public int A { get; set; }
    public int B { get; set; }
    public int Sum { get; set; }
}
