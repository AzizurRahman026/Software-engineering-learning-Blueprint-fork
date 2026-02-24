using Application.Features.Courses.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Chapters.DTOs;

public class ChapterResponseDto
{
    public string SubjectId { get; set; } = string.Empty;
    public string ChapterName { get; set; } = string.Empty;
    public List<LessonDto> Lessons { get; set; } = new();
}
