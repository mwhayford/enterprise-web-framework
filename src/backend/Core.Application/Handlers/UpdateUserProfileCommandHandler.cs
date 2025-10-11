// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Core.Application.Commands;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Application.Mappings;

namespace Core.Application.Handlers;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserDto>
{
    private readonly IUserService _userService;

    public UpdateUserProfileCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UserDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateUserProfileAsync(
            request.UserId,
            request.FirstName,
            request.LastName,
            request.ProfilePictureUrl);

        return user.ToDto();
    }
}
