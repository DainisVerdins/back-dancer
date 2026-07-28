using Application.Interfaces.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories;

public class UserInviteRepository : GenericRepository<UserInvite>, IUserInviteRepository
{
    public UserInviteRepository(DbContext context) : base(context)
    {
    }
}
