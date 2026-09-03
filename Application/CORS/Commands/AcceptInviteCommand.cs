using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.CORS.Commands;

public class AcceptInviteCommand : IRequest<SignInResponseDto>
{
    public required string Token { get; init; }

    public required string Password { get; init; }
}

public class AcceptInviteCommandHandler
    : IRequestHandler<AcceptInviteCommand, SignInResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly ITokenHasherService _tokenHasherService;
    private readonly IJwtTokenService _jwtTokenService;

    public AcceptInviteCommandHandler(
        IUnitOfWork unitOfWork,
        IUserService userService,
        ITokenHasherService tokenHasherService,
        IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
        _tokenHasherService = tokenHasherService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<SignInResponseDto> Handle(
           AcceptInviteCommand request,
           CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException("Invitation token is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required.");

        var tokenHash = _tokenHasherService.Hash(request.Token);

        var invite = await _unitOfWork.UserInvites
            .GetByTokenHashAsync(
                tokenHash,
                cancellationToken);

        if (invite is null)
            throw new NotFoundException("Invitation was not found.");

        if (invite.Status != InviteStatus.Pending)
            throw new ConflictException(
                "Invitation has already been accepted.");

        if (invite.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new ConflictException(
                "Invitation has expired.");

        var existingUser =
            await _userService.GetUserByEmailAsync(invite.Email);

        if (existingUser is not null)
            throw new ConflictException(
                $"User with email '{invite.Email}' already exists.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = new User
            {
                UserName = invite.Email,
                Email = invite.Email,
                EmailConfirmed = true
            };

            var createResult = await _userService.CreateUserAsync(
                user,
                request.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(x => x.Description));

                throw new ValidationException(errors);
            }

            foreach (var role in invite.Roles)
            {
                var roleResult =
                    await _userService.AddRoleToUserByRoleNameAsync(
                        user,
                        role);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        roleResult.Errors.Select(x => x.Description));

                    throw new ValidationException(errors);
                }
            }

            invite.Status = InviteStatus.Accepted;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var claims =
                await _userService.GetClaimsForAccessTokenByUserIdAsync(user.Id);

            var tokenResponse =
                _jwtTokenService.GenerateAccessToken(claims);

            return new SignInResponseDto
            {
                AccessToken = tokenResponse.Token,
                AccessTokenExpiresAt = tokenResponse.ExpiresAt
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(
                CancellationToken.None);

            throw;
        }
    }
}