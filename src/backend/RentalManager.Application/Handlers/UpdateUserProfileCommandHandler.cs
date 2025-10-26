// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Mappings;
using MediatR;

namespace RentalManager.Application.Handlers;

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
