
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Features.Lessons.Command.CreateLesson;

public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand>
{
    private readonly ICourseRepository _courseRepository;
    public CreateLessonCommandHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    public async Task Handle(CreateLessonCommand command, CancellationToken cancellationToken)
    {
        var chapter = await _courseRepository.GetByIdAsync<Chapter>(command.ChapterId);
        if (chapter is null)
        {
            throw new NotFoundException("Chapter not found");
        }
        chapter.Lessons.Add(new Lesson
        {
            Id = command.Lesson.Id,
            LessonName = command.Lesson.LessonName
        });

        await _courseRepository.UpdateAsync(chapter);
    }
}
