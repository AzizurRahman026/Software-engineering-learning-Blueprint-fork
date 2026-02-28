using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Courses.Commands.UpdateCourse;

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand>
{
    private readonly ICourseRepository _courseRepository;
    
    public UpdateCourseCommandHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    
    public async Task Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync<Subject>(request.Id);
        if (course == null)
        {
            throw new NotFoundException();
        }

        course.Name = request.Name;
        course.Description = request.Description;

        var updated = await _courseRepository.UpdateAsync(course);
        if (!updated)
        {
            throw new UnknownException();
        }

        return;
    }
}
