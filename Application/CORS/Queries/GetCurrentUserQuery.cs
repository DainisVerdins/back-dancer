using Application.Constants;
using Application.Dtos;
using Application.Entities.Common;
using Application.Exceptions;
using Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Security.Claims;

namespace Application.CORS.Queries;

public class GetCurrentUserQuery : IRequest<BaseResponse<UserDto>>;
public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, BaseResponse<UserDto>>
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetCurrentUserQueryHandler(IUserService userService, IHttpContextAccessor httpContextAccessor, IRoleService roleService)
    {
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
        _roleService = roleService;
    }

    public async Task<BaseResponse<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (user == null)
            return new BaseResponse<UserDto>(ErrorMessages.GetMessage(ErrorCode.UserNotFound), HttpStatusCode.Unauthorized);

        var contextUser = _httpContextAccessor.HttpContext?.User;

        var activeRole = contextUser?.FindFirst(ClaimTypes.Role)?.Value
                         ?? "NoRoleSelected";

        var availableRoles = await _roleService.GetRolesForUserAsync(user);

        var requiresPasswordChange = contextUser?.HasClaim(c =>
            c.Type == CustomClaimType.ForceChangePassword && c.Value == "true") ?? false;

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            ActiveRole = activeRole,
            AvailableRoles = [.. availableRoles.Select(ur => ur.RoleCode)],
            RequiresPasswordChange = requiresPasswordChange
        };

        return new BaseResponse<UserDto>(userDto);
    }
}
