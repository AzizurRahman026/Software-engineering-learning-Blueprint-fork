
using Application.Features.Chapters.Commands.CreateLessonDetails;
using Domain.Entities;

namespace Application.Features.Chapters.DTOs;

public class LessonDetailsDto
{
    public string Id { get; set; } = string.Empty;
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

public static class LessonDetailsDtoExtension {
    public static LessonDetailsDto ToLessonDetailsDto(this LessonDetails obj)
    {
        return new LessonDetailsDto
        {
            Id = obj.Id,
            LessonId = obj.LessonId,
            Title = obj.Title,
            Description = obj.Description,
            ReferenceUrls = obj.ReferenceUrls
        };
    }
}
