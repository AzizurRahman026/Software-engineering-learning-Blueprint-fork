using MediatR;

namespace Application.Features.Courses.Commands.UpdateCourse;

public class UpdateCourseCommand : IRequest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
