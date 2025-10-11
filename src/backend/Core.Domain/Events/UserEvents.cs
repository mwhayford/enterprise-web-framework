// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace Core.Domain.Events;

public record UserCreatedEvent : BaseEvent
{
    public Guid UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;
}

public record UserUpdatedEvent : BaseEvent
{
    public Guid UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;
}

public record UserDeactivatedEvent : BaseEvent
{
    public Guid UserId { get; init; }

    public string Email { get; init; } = string.Empty;
}
