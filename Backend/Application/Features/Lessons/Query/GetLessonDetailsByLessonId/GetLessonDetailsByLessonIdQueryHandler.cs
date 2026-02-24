
using Application.Common.Interfaces.Repositories;
using Application.Features.Chapters.DTOs;
using Domain.Entities;
using MediatR;
using System.Linq.Expressions;

namespace Application.Features.Lessons.Query.GetLessonDetailsByLessonId;

public class GetLessonDetailsByLessonIdQueryHandler : IRequestHandler<GetLessonDetailsByLessonIdQuery, LessonDetailsDto>
{
    private readonly ICourseRepository _courseRepository;
    public GetLessonDetailsByLessonIdQueryHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<LessonDetailsDto> Handle(GetLessonDetailsByLessonIdQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<LessonDetails, bool>> condition = x => x.LessonId == request.LessonId;
        var lessonDetails = await _courseRepository.GetItemByConditionAsync<LessonDetails>(condition);
        return lessonDetails?.ToLessonDetailsDto()!;
    }
}
