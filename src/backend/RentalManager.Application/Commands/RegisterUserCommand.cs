// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Application.DTOs;
using MediatR;

namespace RentalManager.Application.Commands;

public record RegisterUserCommand : IRequest<UserDto>
{
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? GoogleId { get; init; }

    public string? ProfilePictureUrl { get; init; }
}
