using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Queries.GetUsers;

public class GetUsersQuery : IRequest<List<UserSummaryDto>>
{
    // The requesting user (from the JWT). Must be a SuperAdmin — the list exposes every user's email.
    public string ActingUserId { get; set; } = string.Empty;

    // Optional case-insensitive filter over username/email. Null/empty returns everyone.
    public string? Search { get; set; }
}
