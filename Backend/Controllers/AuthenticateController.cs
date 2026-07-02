using Application.CORS.Commands.Authentication;
using Application.Dtos;
using Application.Entities.Common;
using Application.Exceptions;
using Application.Interfaces.Services;
using Application.ViewModels.Authentication;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/authenticate")]
[ApiVersion("1.0")]
[Authorize]
public class AuthenticateController : ControllerBase
{
    private readonly ILogger<AuthenticateController> _logger;
    private readonly IMediator _mediator;
    private readonly IUserService _userService;
    private readonly IPasswordService _passwordService;

    public AuthenticateController(
        ILogger<AuthenticateController> logger, IMediator mediator,
        IUserService userService, IPasswordService passwordService)
    {
        _logger = logger;
        _mediator = mediator;
        _userService = userService;
        _passwordService = passwordService;
    }

    [HttpPost]
    [Route("sign-in")]
    [ProducesResponseType(typeof(BaseResponse<SignInResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [AllowAnonymous]
    [EnableRateLimiting("FixedPolicy")]
    public async Task<IActionResult> SignIn([FromBody] SignInViewModel model, CancellationToken cancellationToken)
    {
        var passwordValidationErrors = await _passwordService.ValidatePasswordAsync(model.Password);
        if (passwordValidationErrors.Count > 0)
            return StatusCode((int)HttpStatusCode.Unauthorized, new BaseResponse<SignInResponseDto>(null, passwordValidationErrors, HttpStatusCode.BadRequest));

        var response = await _mediator.Send(new SignInUserCommand { Model = model }, cancellationToken);

        if (!response.IsSuccess)
            return StatusCode((int)HttpStatusCode.BadRequest, new BaseResponse<SignInResponseDto>(null, response.ErrorMessages, HttpStatusCode.BadRequest));

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpDelete("sign-out")]
    [ProducesResponseType(typeof(BaseResponse<Unit>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SignOut(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Revoke called");

        var username = HttpContext.User.Identity?.Name;

        if (username is null)
            return StatusCode((int)HttpStatusCode.Unauthorized, new BaseResponse<Unit>(ErrorMessages.GetMessage(ErrorCode.UserNotFound), HttpStatusCode.Unauthorized));

        var response = await _mediator.Send(new SignOutUserCommand(), cancellationToken);

        Response.Cookies.Delete("X-Refresh-Token");

        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse<SignInResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["X-Refresh-Token"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Refresh token cookie not found");
            return StatusCode((int)HttpStatusCode.Unauthorized, new BaseResponse<SignInResponseDto>(ErrorMessages.GetMessage(ErrorCode.RefreshTokenInCookieNotFound), HttpStatusCode.Unauthorized));
        }

        var response = await _mediator.Send(command, cancellationToken);

        return StatusCode((int)response.StatusCode, response);
    }
}
