using Application.Common.Interfaces.Publisher;
using Application.Features.Courses.DTOs;
using Application.Features.Courses.Query.GetAllCourses;
using Application.Features.Courses.Query.GetCourseById;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace API.McpServer.Tools;

[McpServerToolType]
public class CourseTools
{
    private readonly IMessageBus _messageBus;

    public CourseTools(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    [McpServerTool]
    [Description("Gets all available courses. Use when the user asks about available courses, subjects, or what they can learn.")]
    public async Task<string> GetAllCourses()
    {
        var query = new GetAllCoursesQuery();
        var result = await _messageBus.SendAsync<GetAllCoursesQuery, List<CourseResponseDto>>(query);

        return JsonSerializer.Serialize(result,
            new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets details of a specific course by its ID. Use when the user asks about a specific course.")]
    public async Task<string> GetCourseById(
        [Description("The ID of the course to retrieve")]
        string courseId
    )
    {
        var query = new GetCourseByIdQuery { CourseId = courseId };
        var result = await _messageBus.SendAsync<GetCourseByIdQuery, CourseResponseDto>(query);

        return JsonSerializer.Serialize(result,
            new JsonSerializerOptions { WriteIndented = true });
    }
}
