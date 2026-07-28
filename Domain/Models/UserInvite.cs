using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Models;

public class UserInvite : IEntity
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public InviteStatus Status { get; set; }
    public int InvitedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User InvitedByUser { get; set; } = null!;

    public int? AcceptedUserId { get; set; }
    public User? AcceptedUser { get; set; }
}
