using Application.Constants;
using Application.CORS.Commands;
using Application.CORS.Queries;
using Application.Dtos;
using Application.Entities;
using Application.Entities.Common;
using Asp.Versioning;
using Infrastructure.Persistance.Identity.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/me")]
[ApiVersion("1.0")]
[Authorize]
public class MeController : Controller
{
    private readonly IMediator _mediator;

    public MeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get list of available roles for current user
    /// </summary>
    [HttpGet("available-roles")]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<RoleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAvailableRoles()
    {
        var userId = User.FindFirstValue(Application.Constants.CustomClaimType.UserId);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var id = 0;
        if (!int.TryParse(userId, out id))
            return BadRequest("Failed to get id of current user");

        var query = new GetAvailableRolesQuery { UserId = id };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Returns token containing new claims for auth and authorized user with desired role
    /// </summary>
    /// <param name="roleCode"></param>
    /// <returns></returns>
    [HttpPost("select-role")]
    [ProducesResponseType(typeof(BaseResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SelectRole([FromBody] string roleCode)
    {
        var command = new SelectUserRoleCommand{ RoleCode = roleCode };
        var response = await _mediator.Send(command);

        if (!response.IsSuccess)
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }
}
