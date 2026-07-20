using Application.Common.Interfaces.Publisher;
using Application.Features.AiWorker.Commands.Add;
using Application.Features.AiWorker.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiWorkerController : ApiControllerBase
{
    private readonly IMessageBus _messageBus;

    public AiWorkerController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    /// <summary>
    /// Test the AiWorker gRPC round-trip: sends a + b to the gRPC service and
    /// returns the sum it computes. The worker must be running (see AiWorker project).
    /// </summary>
    [AllowAnonymous]
    [HttpGet("add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<AddResultDto>> Add([FromQuery] int a, [FromQuery] int b)
    {
        var result = await _messageBus.SendAsync<AddCommand, AddResultDto>(
            new AddCommand { A = a, B = b });
        return Ok(result);
    }
}
