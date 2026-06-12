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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetCurrentUserQueryHandler(IUserService userService, IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BaseResponse<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetCurrentUserAsync();
        if (user == null)
            return new BaseResponse<UserDto>(ErrorMessages.GetMessage(ErrorCode.UserNotFound), HttpStatusCode.Unauthorized);

        var activeRole = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value
                         ?? "NoRoleSelected";

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            ActiveRole = activeRole
        };

        return new BaseResponse<UserDto>(userDto);
    }
}
