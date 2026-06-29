using Application.CORS.Animal;
using Application.Entities.Common;
using Application.ViewModels.Animal;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/animals")]
[ApiVersion("1.0")]
[Authorize]
public class AnimalController : Controller
{
    private readonly IMediator _mediator;

    public AnimalController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates new animal record in database
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>id of created animal record</returns>
    [HttpPost("")]
    [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAnimal([FromForm] CreateAnimalViewModel model, CancellationToken cancellationToken)
    {
        var command = new CreateAnimalCommand { Model = model };
        var response = await _mediator.Send(command, cancellationToken);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }
}
