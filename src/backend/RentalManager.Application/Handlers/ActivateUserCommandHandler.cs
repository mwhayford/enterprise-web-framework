// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using RentalManager.Application.Commands;
using RentalManager.Application.Interfaces;

namespace RentalManager.Application.Handlers;

public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, bool>
{
    private readonly IUserService _userService;

    public ActivateUserCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        return await _userService.ActivateUserAsync(request.UserId);
    }
}
