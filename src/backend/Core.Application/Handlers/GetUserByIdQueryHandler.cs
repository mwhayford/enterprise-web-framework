using MediatR;
using Core.Application.Queries;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Application.Mappings;

namespace Core.Application.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserService _userService;

    public GetUserByIdQueryHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(request.UserId);
        return user?.ToDto();
    }
}
