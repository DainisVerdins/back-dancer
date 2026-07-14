using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;

namespace Application.CORS.Queries;

public class GetAvailableRolesQuery : IRequest<IEnumerable<RoleDto>>
{
    public required int UserId { get; init; }
}

public class GetAvailableRolesQueryHandler : IRequestHandler<GetAvailableRolesQuery, IEnumerable<RoleDto>>
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

    public async Task<IEnumerable<RoleDto>> Handle(GetAvailableRolesQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId < 0)
            throw new ArgumentException("User id is not valid!");

        var currentUser = await _userService.GetUserByIdAsync(request.UserId);

        if (currentUser is null)
            throw new NotFoundException(ErrorMessages.GetMessage(ErrorCode.UserNotFound));

        var currentUserRoles = await _roleService.GetRolesForUserAsync(currentUser);

        if (currentUserRoles is null)
            return [];

        return currentUserRoles
            .Select(role => _mapper.Map<RoleDto>(role))
            .ToList();
    }
}
