// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Application.DTOs;
using MediatR;

namespace Core.Application.Commands;

public record UpdateUserProfileCommand : IRequest<UserDto>
{
    public Guid UserId { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string? ProfilePictureUrl { get; init; }
}
