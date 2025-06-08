using Application.Dtos;
using Application.Entities.Common;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.ViewModels.Authentication;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Application.CORS.Commands.Authentication;

public class SignInUserCommand : IRequest<BaseResponse<SignInResponseDto>>
{
    [Required]
    public required SignInViewModel Model { get; init; }
}

public class SignInUserCommandHandler : IRequestHandler<SignInUserCommand, BaseResponse<SignInResponseDto>>
{

    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SignInUserCommand> _logger;
    public SignInUserCommandHandler(IUserService userService, IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILogger<SignInUserCommand> logger)
    {

        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<BaseResponse<SignInResponseDto>> Handle(SignInUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByEmailAsync(request.Model.Email);
        if (user is null)
            return new BaseResponse<SignInResponseDto>(null, ErrorMessages.GetMessage(ErrorCode.UserNotFound), HttpStatusCode.NotFound);


        if (_httpContextAccessor?.HttpContext is null)
            return new BaseResponse<SignInResponseDto>(null, ErrorMessages.GetMessage(ErrorCode.NotFound), HttpStatusCode.NotFound);


        if (await _userService.IsLockedOutAsync(user))
        {
            _logger.LogWarning("User {modelEmail} is locked out.", request.Model.Email);

            return new BaseResponse<SignInResponseDto>(null, ErrorMessages.GetMessage(ErrorCode.UserBlocked), HttpStatusCode.Unauthorized);
        }

        if (!await _userService.CheckPasswordAsync(user, request.Model.Password))
        {
            await _userService.IncrementAccessFailedCountAsync(user);

            return new BaseResponse<SignInResponseDto>(null, ErrorMessages.GetMessage(ErrorCode.InvalidPassword), HttpStatusCode.BadRequest);
        }

        await _userService.ResetAccessFailedCountAsync(user);

        var claimsToAdd = await _userService.GetClaimsForAccessTokenByUserIdAsync(user.Id);

        var accessToken = _jwtTokenService.GenerateAccessToken(claimsToAdd);
        var refreshToken = _jwtTokenService.GetRefreshToken();

        _unitOfWork.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            IsRevoked = false,
            UserId = user.Id
        }
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Refresh-Token", refreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = refreshToken.ExpiresAt
        });

        return new BaseResponse<SignInResponseDto>(new SignInResponseDto
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
        }, HttpStatusCode.OK);
    }
}
