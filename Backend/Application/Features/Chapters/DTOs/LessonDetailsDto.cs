
using Application.Features.Chapters.Commands.CreateLessonDetails;

namespace Application.Features.Chapters.DTOs;

public class LessonDetailsDto
{
    public string LessonId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> ReferenceUrls { get; set; } = new();

    public CreateLessonDetailsCommand ToCreateLessonDetailsCommand()
    {
        return new CreateLessonDetailsCommand
        {
            LessonId = LessonId,
            Title = Title,
            Description = Description,
            ReferenceUrls = ReferenceUrls
        };
    }
}
