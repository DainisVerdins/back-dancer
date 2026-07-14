using Application.CORS.Commands;
using Application.CORS.Queries;
using Application.Dtos;
using Application.Entities;
using Application.Exceptions;
using Application.ViewModels;
using Asp.Versioning;
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
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAvailableRoles(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(Application.Constants.CustomClaimType.UserId);

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedException("Claims does not contain userId!");

        var id = 0;
        if (!int.TryParse(userId, out id))
            throw new ApplicationException("Failed to get id of current user");

        var result = await _mediator.Send(new GetAvailableRolesQuery { UserId = id }, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Returns token containing new claims for auth and authorized user with desired role
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("select-role")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SelectRole([FromBody] SelectRoleViewModel model, CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new SelectUserRoleCommand { RoleCode = model.RoleCode }, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Returns current user information
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>User Profile</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new GetCurrentUserQuery(), cancellationToken);

        return Ok(response);
    }
}
