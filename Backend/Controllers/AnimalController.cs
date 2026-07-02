using Application.Constants;
using Application.CORS.Animal;
using Application.CORS.Queries;
using Application.Dtos.Animal;
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
    [Authorize(Roles = $"{UserRole.SuperAdmin}, {UserRole.Admin}, {UserRole.ShelterWorker}")]
    public async Task<IActionResult> CreateAnimal([FromForm] CreateAnimalViewModel model, CancellationToken cancellationToken)
    {
        var command = new CreateAnimalCommand { Model = model };
        var response = await _mediator.Send(command, cancellationToken);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BaseResponse<PaginatedList<AnimalDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnimalDto>> GetAnimal(int id, CancellationToken ct = default)
    {
        var query = new GetAnimalWithImagesQuery { AnimalId = id };
        var response = await _mediator.Send(query, ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }

    [HttpGet("")]
    [ProducesResponseType(typeof(BaseResponse<PaginatedList<AnimalDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedList<AnimalDto>>> GetAllAnimalsForTable(
    [FromQuery] PaginationParams paging,
    [FromQuery] AnimalFilterViewModel filter,
    CancellationToken ct = default)
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var query = new GetAllAnimalsQuery { Filter = filter, Paging = paging };
        var response = await _mediator.Send(query, ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }

    [HttpPut("")]
    [ProducesResponseType(typeof(BaseResponse<Unit>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = $"{UserRole.SuperAdmin}, {UserRole.Admin}, {UserRole.ShelterWorker}")]
    public async Task<ActionResult<Unit>> UpdateAnimal(
    [FromForm] UpdateAnimalViewModel model,
    CancellationToken ct = default)
    {
        var command = new UpdateAnimalCommand { Model = model };
        var response = await _mediator.Send(command, ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{UserRole.SuperAdmin}, {UserRole.Admin}, {UserRole.ShelterWorker}")]
    [ProducesResponseType(typeof(BaseResponse<Unit>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Unit>> DeleteAnimal(
    [FromRoute] int id,
    CancellationToken ct = default)
    {
        var command = new DeleteAnimalCommand { Id = id };
        var response = await _mediator.Send(command, ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }
}
