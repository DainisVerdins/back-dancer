using Application.CORS.Queries;
using Application.Dtos.Animal;
using Application.Entities.Common;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace WebApi.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/v{version:apiVersion}/public")]
[ApiVersion("1.0")]
public class PublicController : Controller
{
    private readonly IMediator _mediator;

    public PublicController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("animals")]
    [EnableRateLimiting("FixedPolicy")]
    [ProducesResponseType(typeof(BaseResponse<PaginatedList<PublicAnimalDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedList<AnimalDto>>> GetPaginatedPublicAnimal(
    [FromQuery] PaginationParams paging,
    CancellationToken ct = default)
    {
        var query = new GetPublicAnimalsPagedQuery { Paging = paging };
        var response = await _mediator.Send(query, ct);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }
}
