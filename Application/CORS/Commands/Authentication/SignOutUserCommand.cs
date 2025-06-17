using Application.Entities.Common;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.CORS.Commands.Authentication;

public class SignOutUserCommand : IRequest<BaseResponse<Unit>>
{
}

public class SignOutUserCommandHandler : IRequestHandler<SignOutUserCommand, BaseResponse<Unit>>
{

    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    public SignOutUserCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    {

        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<Unit>> Handle(SignOutUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (user is null)
            return new BaseResponse<Unit>(Unit.Value, ErrorMessages.GetMessage(ErrorCode.UserNotFound), HttpStatusCode.NotFound);

        var userRefreshTokens = _unitOfWork.RefreshTokens.Find(x => x.UserId == user.Id && !x.IsRevoked).ToList();

        foreach (var userRefreshToken in userRefreshTokens)
            userRefreshToken.IsRevoked = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BaseResponse<Unit>(Unit.Value, HttpStatusCode.OK);
    }
}