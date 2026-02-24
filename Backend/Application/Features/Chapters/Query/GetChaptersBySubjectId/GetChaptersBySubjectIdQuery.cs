
using Application.Features.Courses.DTOs;
using MediatR;

namespace Application.Features.Chapters.Query.GetChaptersBySubjectId;

public class GetChaptersBySubjectIdQuery : IRequest<List<ChapterResponseDto>>
{
    public string SubjectId { get; set; } = string.Empty;
}
