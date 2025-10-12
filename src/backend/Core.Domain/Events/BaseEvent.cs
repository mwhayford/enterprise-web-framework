// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Domain.Interfaces;
using MediatR;

namespace Core.Domain.Events;

public abstract record BaseEvent : IDomainEvent, INotification
{
    protected BaseEvent()
    {
        EventType = GetType().Name;
    }

    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public string EventType { get; init; } = string.Empty;

    public string? CorrelationId { get; init; }

    public string? CausationId { get; init; }
}
