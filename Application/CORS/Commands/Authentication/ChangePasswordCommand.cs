using Application.Exceptions;
using Application.Interfaces.Services;
using MediatR;

namespace Application.CORS.Commands.Authentication;

public record ChangePasswordCommand(string OldPassword, string NewPassword) : IRequest;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IUserService _userService;

    public ChangePasswordCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (user == null)
            throw new NotFoundException(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        var result = await _userService.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (result.Succeeded)
            return;

        var errors = result.Errors.Select(e => e.Description).ToList();
        throw new ApplicationException(string.Join(",", errors));
    }
}