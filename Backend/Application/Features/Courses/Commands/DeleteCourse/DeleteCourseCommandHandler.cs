using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Courses.Commands.DeleteCourse;


public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand>
{
    private readonly ICourseRepository _courseRepository;
    public DeleteCourseCommandHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    public async Task Handle(DeleteCourseCommand command, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync<Subject>(command.Id);
        if (course is null)
        {
            throw new NotFoundException();
        }
        var deleted = await _courseRepository.DeleteByIdAsync<Subject>(command.Id);
        if (!deleted)
        {
            throw new UnknownException();
        }
    }
}
