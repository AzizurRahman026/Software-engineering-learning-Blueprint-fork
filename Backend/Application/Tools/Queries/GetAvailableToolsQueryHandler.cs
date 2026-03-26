
using Application.Common.Interfaces.Services;
using Application.Tools.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Tools.Queries;

public class GetAvailableToolsQueryHandler : IRequestHandler<GetAvailableToolsQuery, List<ToolSummary>>
{
    private readonly IMcpService _mcpService;
    public GetAvailableToolsQueryHandler(IMcpService mcpService)
    {
        _mcpService = mcpService;
    }

    public async Task<List<ToolSummary>> Handle(GetAvailableToolsQuery request, CancellationToken ct)
    {
        var tools = await _mcpService.GetToolsAsync(ct);
        return tools.Select(t => new ToolSummary
        {
            Name = t.Name,
            Description = t.Description
        }).ToList();
    }
}
