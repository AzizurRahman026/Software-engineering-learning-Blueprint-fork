
using Application.Features.Courses.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Features.Courses.Query.GetCourseById;

public class GetCourseByIdQuery : IRequest<CourseResponseDto>
{
    public string CourseId { get; set; } = string.Empty;
}
