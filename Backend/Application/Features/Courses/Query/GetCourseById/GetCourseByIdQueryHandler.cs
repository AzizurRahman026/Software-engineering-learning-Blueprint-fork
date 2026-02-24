
using Application.Common.Interfaces.Repositories;
using Application.Features.Courses.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Features.Courses.Query.GetCourseById;

public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, CourseResponseDto>
{
    private readonly ICourseRepository _courseRepository;
    public GetCourseByIdQueryHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    public async Task<CourseResponseDto> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        var subject = await _courseRepository.GetByIdAsync<Subject>(request.CourseId);
        return subject?.ToCourseResponseDto()!;
    }
}
