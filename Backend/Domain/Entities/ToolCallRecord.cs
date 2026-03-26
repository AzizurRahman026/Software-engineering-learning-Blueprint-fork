
namespace Domain.Entities;

public class ToolCallRecord
{
    public string ToolName { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;

    public static ToolCallRecord Create(string toolName, string result)
        => new() { ToolName = toolName, Result = result };
}
