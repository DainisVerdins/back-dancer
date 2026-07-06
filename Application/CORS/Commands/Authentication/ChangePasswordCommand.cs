using Application.Entities.Common;
using Application.Exceptions;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.CORS.Commands.Authentication;

public record ChangePasswordCommand(string OldPassword, string NewPassword) : IRequest<BaseResponse<Unit>>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, BaseResponse<Unit>>
{
    private readonly IUserService _userService;

    public ChangePasswordCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<BaseResponse<Unit>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (user == null)
            return new BaseResponse<Unit>(ErrorMessages.GetMessage(ErrorCode.UserNotFound), HttpStatusCode.Unauthorized);

        var result = await _userService.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return new BaseResponse<Unit>(Unit.Value, errors, HttpStatusCode.BadRequest);
        }

        return new BaseResponse<Unit>(Unit.Value);
    }
}