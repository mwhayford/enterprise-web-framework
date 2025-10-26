// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Domain.Events;

public record PropertyListedEvent : BaseEvent
{
    public Guid PropertyId { get; init; }

    public Guid OwnerId { get; init; }

    public string Address { get; init; } = default!;

    public decimal MonthlyRent { get; init; }

    public DateTime AvailableDate { get; init; }
}

