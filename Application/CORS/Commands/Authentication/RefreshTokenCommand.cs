using Application.Dtos;
using Application.Entities.Common;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Application.CORS.Commands.Authentication;

public class RefreshTokenCommand : IRequest<BaseResponse<SignInResponseDto>>
{
    public string? ActiveRole { get; init; }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, BaseResponse<SignInResponseDto>>
{
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RefreshTokenCommandHandler(
        IUserService userService, IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork, IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<BaseResponse<SignInResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor?.HttpContext is null)
            return new BaseResponse<SignInResponseDto>(ErrorMessages.GetMessage(ErrorCode.NotFound), HttpStatusCode.NotFound);

        if (!_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            return new BaseResponse<SignInResponseDto>(
                ErrorMessages.GetMessage(ErrorCode.RefreshTokenInvalid),
                HttpStatusCode.Unauthorized);


        var storedToken = await _unitOfWork.RefreshTokens.GetRefreshTokenAsync(refreshToken, cancellationToken);
        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            return new BaseResponse<SignInResponseDto>(
                ErrorMessages.GetMessage(ErrorCode.RefreshTokenInvalid),
                HttpStatusCode.Unauthorized);

        var user = await _userService.GetUserByIdAsync(storedToken.UserId);
        if (user is null)
            return new BaseResponse<SignInResponseDto>(
                ErrorMessages.GetMessage(ErrorCode.UserNotFound),
                HttpStatusCode.NotFound);


        if (!string.IsNullOrEmpty(request.ActiveRole))
        {
            var isUserInRole = await _userService.IsInRoleAsync(user, request.ActiveRole);
            if (!isUserInRole)
            {
                return new BaseResponse<SignInResponseDto>(
                    ErrorMessages.GetMessage(ErrorCode.OperationFailed),
                    HttpStatusCode.Forbidden);
            }
        }

        var claimsToAdd = await _userService.GetClaimsForAccessTokenByUserIdAsync(user.Id, request.ActiveRole);

        var newAccessToken = _jwtTokenService.GenerateAccessToken(claimsToAdd);
        var newRefreshToken = _jwtTokenService.GetRefreshToken();

        var refreshTokenToAdd = _mapper.Map<RefreshToken>(newRefreshToken);
        refreshTokenToAdd.UserId = user.Id;
        storedToken.IsRevoked = true;

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenToAdd, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Refresh-Token", refreshTokenToAdd.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = refreshTokenToAdd.ExpiresAt
        });

        return new BaseResponse<SignInResponseDto>(
            new SignInResponseDto
            {
                AccessToken = newAccessToken.Token,
                AccessTokenExpiresAt = newAccessToken.ExpiresAt
            },
            HttpStatusCode.OK);
    }
}