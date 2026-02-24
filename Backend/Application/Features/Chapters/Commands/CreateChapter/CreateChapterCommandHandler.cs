
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.Chapters.Commands.CreateChapter;

public class CreateChapterCommandHandler : IRequestHandler<CreateChapterCommand>
{
    private readonly ICourseRepository _courseRepository;
    public CreateChapterCommandHandler(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }
    public async Task Handle(CreateChapterCommand command, CancellationToken cancellationToken)
    {
        var chapter = await _courseRepository.GetByIdAsync<Chapter>(command.ChapterId);
        if (chapter is null)
        {
            // create new chapter
            chapter = new Chapter
            {
                SubjectId = command.SubjectId,
                ChapterName = command.ChapterName,
                Lessons = new()
            };
            if (!string.IsNullOrWhiteSpace(command.ChapterId))
            {
                chapter.Id = command.ChapterId;
            }
        }
        chapter.Lessons.Add(new Lesson
        {
            Id = command.Lesson.Id,
            LessonName = command.Lesson.LessonName
        });

        await _courseRepository.AddAsync(chapter);
    }
}
