using MediatR;
using Core.Application.Commands;
using Core.Application.Interfaces;

namespace Core.Application.Handlers;

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, bool>
{
    private readonly IUserService _userService;

    public DeactivateUserCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        return await _userService.DeactivateUserAsync(request.UserId);
    }
}
