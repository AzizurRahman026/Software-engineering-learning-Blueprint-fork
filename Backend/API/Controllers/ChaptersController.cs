using Application.Common.Interfaces.Publisher;
using Application.Features.Chapters.Commands.CreateChapter;
using Application.Features.Chapters.Commands.DeleteChapter;
using Application.Features.Chapters.Commands.UpdateChapter;
using Application.Features.Chapters.Query.GetChaptersBySubjectId;
using Application.Features.Courses.DTOs;
using Application.Features.Courses.Query.GetCourseById;
using Application.Features.Lessons.Command.CreateLesson;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChaptersController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    public ChaptersController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    [HttpGet("{subjectId}")]
    public async Task<ActionResult<IEnumerable<ChapterResponseDto>>> GetChaptersBySubject(string subjectId)
    {
        var query = new GetChaptersBySubjectIdQuery
        {
            SubjectId = subjectId
        };
        var response = await _messageBus.SendAsync<GetChaptersBySubjectIdQuery, List<ChapterResponseDto>>(query);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> CreateChapter([FromBody] ChapterDto chapter)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var command = chapter.ToCreateChapterCommand();
        await _messageBus.SendAsync<CreateChapterCommand>(command);
        return Created();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateChapter(string id, [FromBody] ChapterDto chapter)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new UpdateChapterCommand { Id = id, ChapterName = chapter.ChapterName };
        await _messageBus.SendAsync<UpdateChapterCommand>(command);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteChapter(string id)
    {
        var command = new DeleteChapterCommand { Id = id };
        await _messageBus.SendAsync(command);
        return NoContent();
    }
}
