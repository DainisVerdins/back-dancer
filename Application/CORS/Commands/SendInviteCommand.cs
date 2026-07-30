using Application.Constants;
using Application.Entities;
using Application.Entities.Templates;
using Application.Exceptions;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Settings;
using Domain.Enums;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Cryptography;

namespace Application.CORS.Commands;

public class SendInviteCommand : IRequest<int>
{
    public required string Email { get; init; }
    public required List<string> Roles { get; init; }
    public required int InvitedByUserId { get; init; }
}
public class SendInviteCommandHandler : IRequestHandler<SendInviteCommand, int>
{
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenHasherService _tokenHasherService;
    private readonly DomainSettings _domainSettings;
    private readonly IEmailService _emailService;

    public SendInviteCommandHandler(
        IUserService userService,
        IUnitOfWork unitOfWork,
        ITokenHasherService tokenHasherService,
        IEmailService emailService,
        IOptions<DomainSettings> domainSettings)
    {
        _userService = userService;
        _unitOfWork = unitOfWork;
        _tokenHasherService = tokenHasherService;
        _domainSettings = domainSettings.Value;
        _emailService = emailService;
    }

    public async Task<int> Handle(
        SendInviteCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByEmailAsync(request.Email);
        if (user is not null)
            throw new ConflictException(
                $"User with email '{request.Email}' already exists.");

        var existingInvite =
            await _unitOfWork.UserInvites.GetPendingByEmailAsync(
                request.Email,
                cancellationToken);

        if (existingInvite is not null)
            throw new ConflictException(
                $"An active invitation for email '{request.Email}' already exists.");

        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var plainToken = Convert.ToBase64String(tokenBytes);

        var tokenHash = _tokenHasherService.Hash(plainToken);

        var now = DateTimeOffset.UtcNow;

        var invite = new UserInvite
        {
            Email = request.Email,
            Roles = [.. request.Roles],
            Token = tokenHash,
            InvitedByUserId = request.InvitedByUserId,
            Status = InviteStatus.Pending,
            CreatedAt = now,
            ExpiresAt = now.AddHours(48)
        };

        await _unitOfWork.UserInvites.AddAsync(invite, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);


        var inviteLink = $"{_domainSettings.FrontendDomain.TrimEnd('/')}/register?token={Uri.EscapeDataString(plainToken)}";

        var templateData = new InviteTemplateData
        {
            InviteLink = inviteLink,
            ExpiresAtText = invite.ExpiresAt.ToString(
                "dd.MM.yyyy HH:mm 'UTC'",
                CultureInfo.InvariantCulture)
        };

        await _emailService.SendTemplateEmailAsync(
            new TemplateEmailMessage
            {
                ToEmail = invite.Email,
                TemplateData = templateData
            },
            EmailTemplate.UserInvite,
            cancellationToken);

        return invite.Id;
    }
}