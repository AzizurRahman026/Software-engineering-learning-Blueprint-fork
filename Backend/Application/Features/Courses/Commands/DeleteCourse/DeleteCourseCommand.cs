
using MediatR;

namespace Application.Features.Courses.Commands.DeleteCourse;

public class DeleteCourseCommand : IRequest
{
    public string Id { get; set; } = string.Empty;
}
