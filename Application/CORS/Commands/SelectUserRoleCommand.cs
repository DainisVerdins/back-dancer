using Application.Constants;
using Application.Entities;
using Application.Entities.Common;
using Application.Exceptions;
using Application.Interfaces.Services;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Claims;

namespace Application.CORS.Commands;

public class SelectUserRoleCommand : IRequest<BaseResponse<TokenResponse>>
{
    public required string RoleCode { get; init; }
}

public class SelectUserRoleCommandHandler : IRequestHandler<SelectUserRoleCommand, BaseResponse<TokenResponse>>
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

    public async Task<BaseResponse<TokenResponse>> Handle(SelectUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (user == null)
            return new BaseResponse<TokenResponse>(ErrorMessages.GetMessage(ErrorCode.UserNotFound), HttpStatusCode.Unauthorized);

        if (!UserRole.GetRoleNames().Contains(request.RoleCode))
            return new BaseResponse<TokenResponse>(ErrorMessages.GetApiErrorMessage(ApiErrorCode.EntityDoesNotExist), HttpStatusCode.BadRequest);

        var isUserInRole = await _userManager.IsInRoleAsync(user, request.RoleCode);
        if (!isUserInRole)
            return new BaseResponse<TokenResponse>(ErrorMessages.GetMessage(ErrorCode.OperationFailed), HttpStatusCode.Forbidden);

        var claims = new List<Claim>
        {
            new(CustomClaimType.UserId, user.Id.ToString()),
            new(CustomClaimType.UserName, user.UserName ?? user.Email ?? ""),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
            new(ClaimTypes.Email, user.Email ?? "")
        };

        var userClaims = await _userManager.GetClaimsAsync(user);
        if (userClaims != null)
            claims.AddRange(userClaims);

        claims.Add(new Claim(CustomClaimType.RoleName, request.RoleCode));
        claims.Add(new Claim(ClaimTypes.Role, request.RoleCode));

        var tokenResponse = _jwtTokenService.GenerateAccessToken(claims);

        return new BaseResponse<TokenResponse>(tokenResponse);
    }
}