using Application.Common.Interfaces.Publisher;
using Application.Features.Chapters.Commands.CreateLessonDetails;
using Application.Features.Chapters.DTOs;
using Application.Features.Lessons.Query.GetLessonDetailsByLessonId;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonDetailsController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    public LessonDetailsController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    [HttpGet("{subjectId}/lesson/{lessonId}")]
    public async Task<ActionResult<LessonDetails>> GetLessonDetailsByLessonId(string subjectId, string lessonId)
    {
        var query = new GetLessonDetailsByLessonIdQuery
        {
            LessonId = lessonId
        };
        var lessonDetails = await _messageBus.SendAsync<GetLessonDetailsByLessonIdQuery, LessonDetailsDto>(query);
        
        if (lessonDetails == null)
            return NotFound();

        return Ok(lessonDetails);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LessonDetails>> GetLessonDetailsById(string id)
    {
        var lessonDetails = new LessonDetails
        {
            Id = id,
            LessonId = "1",
            Title = "Introduction to Linear Equations",
            Description = "Learn the fundamentals of linear equations and how to solve them.",
            ReferenceUrls = new List<string>
            {
                "https://example.com/linear-equations"
            }
        };

        if (lessonDetails == null)
            return NotFound();

        return Ok(lessonDetails);
    }

    [HttpPost]
    public async Task<ActionResult> CreateLessonDetails([FromBody] LessonDetailsDto lessonDetails)
    {
        var command = lessonDetails.ToCreateLessonDetailsCommand();
        await _messageBus.SendAsync<CreateLessonDetailsCommand>(command);
        
        return Created();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<LessonDetails>> UpdateLessonDetails(string id, [FromBody] LessonDetails lessonDetails)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != lessonDetails.Id)
            return BadRequest("ID mismatch");

        return Ok(lessonDetails);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteLessonDetails(string id)
    {
        return NoContent();
    }
}
