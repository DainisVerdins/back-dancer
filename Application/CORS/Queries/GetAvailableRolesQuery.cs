using Application.Dtos;
using Application.Entities.Common;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;
using System.Net;

namespace Application.CORS.Queries;

public class GetAvailableRolesQuery : IRequest<BaseResponse<IEnumerable<RoleDto>>>
{
    public required int UserId { get; init; }
}

public class GetAvailableRolesQueryHandler : IRequestHandler<GetAvailableRolesQuery, BaseResponse<IEnumerable<RoleDto>>>
{
    private readonly IRoleService _roleService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public GetAvailableRolesQueryHandler(
       IUserService userService, IMapper mapper, IRoleService roleService)
    {
        _userService = userService;
        _mapper = mapper;
        _roleService = roleService;
    }

    public async Task<BaseResponse<IEnumerable<RoleDto>>> Handle(GetAvailableRolesQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId < 0)
            throw new ArgumentException("User id is not valid!");

        var currentUser = await _userService.GetUserByIdAsync(request.UserId);

        if (currentUser is null)
            return new BaseResponse<IEnumerable<RoleDto>>(null, "err message ", HttpStatusCode.BadRequest);


        var currentUserRoles = await _roleService.GetRolesForUserAsync(currentUser);

        if (currentUserRoles is null)
            return new BaseResponse<IEnumerable<RoleDto>>([], HttpStatusCode.OK);

        var output = currentUserRoles
            .Select(role => _mapper.Map<RoleDto>(role))
            .ToList();


        return new BaseResponse<IEnumerable<RoleDto>>(output, HttpStatusCode.OK);
    }
}
