
using Application.Features.Chapters.DTOs;
using MediatR;

namespace Application.Features.Lessons.Query.GetLessonDetailsByLessonId;

public class GetLessonDetailsByLessonIdQuery : IRequest<LessonDetailsDto>
{
    public string LessonId { get; set; } = string.Empty;
}
