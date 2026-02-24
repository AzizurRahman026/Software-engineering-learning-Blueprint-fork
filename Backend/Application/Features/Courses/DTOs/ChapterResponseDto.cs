using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Courses.DTOs;

public class ChapterResponseDto
{
    public string ChapterId { get; set; } = string.Empty;
    public string ChapterName { get; set; } = string.Empty;
    public List<LessonResponseDto> Lessons { get; set; } = new();
}

public class LessonResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string LessonName { get; set; } = string.Empty;
}

public class LessonDetailsResponseDto
{
    public string Id { get; set; }
    public string LessonId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> ReferenceUrls { get; set; } = new();
}
