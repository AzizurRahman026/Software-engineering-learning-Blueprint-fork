
using Application.Tools.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Tools.Queries;

public class GetAvailableToolsQuery : IRequest<List<ToolSummary>>
{
}
