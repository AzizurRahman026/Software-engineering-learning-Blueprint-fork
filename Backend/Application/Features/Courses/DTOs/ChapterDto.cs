using Application.Features.Chapters.Commands.CreateChapter;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Courses.DTOs;

public class ChapterDto
{
    public string SubjectId { get; set; } = string.Empty;
    public string ChapterId { get; set; } = string.Empty;
    public string ChapterName { get; set; } = string.Empty;
    public LessonDto Lesson { get; set; } = new();

    public CreateChapterCommand ToCreateChapterCommand()
    {
        return new CreateChapterCommand
        {
            SubjectId = this.SubjectId,
            ChapterId = this.ChapterId,
            ChapterName = this.ChapterName,
            Lesson = this.Lesson
        };
    }
}

public class LessonDto
{
    public string Id { get; set; } = string.Empty;
    public string LessonName { get; set; } = string.Empty;
}
