
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.Chapters.Commands.CreateLessonDetails;

public class CreateLessonDetailsCommandHandler : IRequestHandler<CreateLessonDetailsCommand>
{
    private readonly ICourseRepository _courseRepository;
    public CreateLessonDetailsCommandHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task Handle(CreateLessonDetailsCommand request, CancellationToken cancellationToken)
    {
        // find lesson by id
        // add lesson details
        var lessonDetails = new LessonDetails
        {
            LessonId = request.LessonId,
            Title = request.Title,
            Description = request.Description,
            ReferenceUrls = request.ReferenceUrls
        };
        await _courseRepository.AddAsync(lessonDetails);
    }
}
