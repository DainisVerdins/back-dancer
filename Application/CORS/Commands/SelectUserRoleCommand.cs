using Application.Constants;
using Application.Entities;
using Application.Exceptions;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.CORS.Commands;

public class SelectUserRoleCommand : IRequest<TokenResponse>
{
    public required string RoleCode { get; init; }
}

public class SelectUserRoleCommandHandler : IRequestHandler<SelectUserRoleCommand, TokenResponse>
{
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly UserManager<User> _userManager;

    public SelectUserRoleCommandHandler(
        IUserService userService,
        IJwtTokenService jwtTokenService,
        UserManager<User> userManager)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _userManager = userManager;
    }

    public async Task<TokenResponse> Handle(SelectUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (user == null)
            throw new NotFoundException(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        if (!UserRole.GetRoleNames().Contains(request.RoleCode))
            throw new ValidationException(ErrorMessages.GetApiErrorMessage(ApiErrorCode.EntityDoesNotExist));

        var isUserInRole = await _userManager.IsInRoleAsync(user, request.RoleCode);
        if (!isUserInRole)
            throw new ValidationException($"User does not have provided role: {request.RoleCode}");

        var claims = await _userService.GetClaimsForAccessTokenByUserIdAsync(user.Id, request.RoleCode);

        return _jwtTokenService.GenerateAccessToken(claims);
    }
}