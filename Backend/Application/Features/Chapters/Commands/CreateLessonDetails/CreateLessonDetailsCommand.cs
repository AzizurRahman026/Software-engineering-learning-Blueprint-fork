
using MediatR;

namespace Application.Features.Chapters.Commands.CreateLessonDetails;

public class CreateLessonDetailsCommand : IRequest
{
    public string LessonId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> ReferenceUrls { get; set; } = new();
}
