// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Core.Application.Commands;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Application.Mappings;

namespace Core.Application.Handlers;

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
            Core.Domain.ValueObjects.Email.Create(request.Email),
            request.GoogleId,
            request.ProfilePictureUrl);

        return user.ToDto();
    }
}
