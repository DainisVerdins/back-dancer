using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using MediatR;

namespace Application.CORS.Commands.Authentication;

public class SignOutUserCommand : IRequest
{
}

public class SignOutUserCommandHandler : IRequestHandler<SignOutUserCommand>
{

    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    public SignOutUserCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    {

        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SignOutUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (user is null)
            throw new NotFoundException(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        var userRefreshTokens = _unitOfWork.RefreshTokens.Find(x => x.UserId == user.Id && !x.IsRevoked).ToList();

        foreach (var userRefreshToken in userRefreshTokens)
            userRefreshToken.IsRevoked = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}