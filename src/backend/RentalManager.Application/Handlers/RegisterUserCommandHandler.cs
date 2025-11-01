// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Mappings;

namespace RentalManager.Application.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserDto>
{
    private readonly IUserService _userService;

    public RegisterUserCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.RegisterUserAsync(
            request.FirstName,
            request.LastName,
            RentalManager.Domain.ValueObjects.Email.Create(request.Email),
            request.GoogleId,
            request.ProfilePictureUrl);

        return user.ToDto();
    }
}
