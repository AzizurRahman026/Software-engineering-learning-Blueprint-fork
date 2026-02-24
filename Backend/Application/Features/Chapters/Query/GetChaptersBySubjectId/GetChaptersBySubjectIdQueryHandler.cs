using Application.Common.Interfaces.Repositories;
using Application.Features.Courses.DTOs;
using Domain.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Application.Features.Chapters.Query.GetChaptersBySubjectId;

public class GetChaptersBySubjectIdQueryHandler : IRequestHandler<GetChaptersBySubjectIdQuery, List<ChapterResponseDto>>
{
    private readonly ICourseRepository _courseRepository;
    public GetChaptersBySubjectIdQueryHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    public async Task<List<ChapterResponseDto>> Handle(GetChaptersBySubjectIdQuery query, CancellationToken cancellationToken)
    {
        Expression<Func<Chapter, bool>> condition = o => o.SubjectId == query.SubjectId;
        var chapters = await _courseRepository.GetItemsByConditionAsync<Chapter>(condition);
        var chapterDtos = chapters?.Select(o =>
            new ChapterResponseDto
            {
                ChapterId = o.Id,
                ChapterName = o.ChapterName,
                Lessons = o.Lessons.Select(l =>
                    new LessonResponseDto
                    {
                        Id = l.Id,
                        LessonName = l.LessonName
                    }
                ).ToList()
            }
        ).ToList();
        return chapterDtos;
    }
}