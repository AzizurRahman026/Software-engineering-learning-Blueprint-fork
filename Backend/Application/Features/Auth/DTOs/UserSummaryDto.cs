using Domain.Entities;

namespace Application.Features.Auth.DTOs;

/// <summary>
/// Safe projection of a user for admin listing (role management). Deliberately excludes
/// password hashes, salts, and token hashes — only identity + role are exposed.
/// </summary>
public class UserSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public static UserSummaryDto FromEntity(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email.Value,
        Role = u.Role.ToString()
    };
}
