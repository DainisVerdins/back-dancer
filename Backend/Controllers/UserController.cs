using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/user")]
[ApiVersion("1.0")]
[Authorize]
public class UserController : Controller
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    //[HttpPost]
    //[AllowAnonymous]
    //[Route("recover-password")]
    //[ProducesResponseType(typeof(BaseResponse<Unit>), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //[EnableRateLimiting("FixedPolicy")]
    //public async Task<IActionResult> RecoverPassword([FromBody] RecoverPasswordViewModel model, CancellationToken cancellationToken)
    //{
    //    var response = await _mediator.Send(new RecoverUserPasswordCommand { Model = model }, cancellationToken);

    //    return StatusCode((int)response.StatusCode, response);
    //}

    //[AllowAnonymous]
    //[HttpPost("confirm-password")]
    //[ProducesResponseType(typeof(BaseResponse<Unit>), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //[EnableRateLimiting("FixedPolicy")]
    //public async Task<IActionResult> ConfirmPasswordChange([FromBody] ConfirmPasswordChangeModel model, CancellationToken cancellationToken)
    //{
    //    var response = await _mediator.Send(new ConfirmUserPasswordChangeCommand { Model = model }, cancellationToken);

    //    return StatusCode((int)response.StatusCode, response);
    //}

    //[HttpPost("change-password")]
    //[ProducesResponseType(typeof(BaseResponse<Unit>), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model, CancellationToken cancellationToken)
    //{
    //    var response = await _mediator.Send(new ChangeUserPasswordCommand { Model = model }, cancellationToken);

    //    return StatusCode((int)response.StatusCode, response);
    //}

    //[HttpGet("{userId:guid}")]
    //[ProducesResponseType(typeof(BaseResponse<UserDto>), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status410Gone)]
    //[Authorize(Roles = UserRole.Admin)]
    //public async Task<IActionResult> GetUserById([FromRoute] Guid userId, CancellationToken cancellationToken)
    //{
    //    var response = await _mediator.Send(new GetUserByIdQuery { UserId = userId }, cancellationToken);

    //    return StatusCode((int)response.StatusCode, response);
    //}

    //[HttpGet("current")]
    //[ProducesResponseType(typeof(BaseResponse<CurrentUserDto>), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status410Gone)]
    //public async Task<IActionResult> GetCurrentUserInformation(CancellationToken cancellationToken)
    //{
    //    var response = await _mediator.Send(new GetCurrentUserInformationQuery(), cancellationToken);

    //    return StatusCode((int)response.StatusCode, response);
    //}
}
