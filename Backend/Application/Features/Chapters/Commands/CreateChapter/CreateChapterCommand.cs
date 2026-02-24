
using Application.Features.Courses.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Features.Chapters.Commands.CreateChapter;

public class CreateChapterCommand : IRequest
{
    public string SubjectId { get; set; } = string.Empty;
    public string ChapterId { get; set; } = string.Empty;
    public string ChapterName { get; set; } = string.Empty;
    public LessonDto Lesson { get; set; } = new();
}


