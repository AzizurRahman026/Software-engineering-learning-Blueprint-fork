using MediatR;

namespace Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommand : IRequest
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
