// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Domain.Events;

public record PropertyApplicationSubmittedEvent : BaseEvent
{
    public Guid ApplicationId { get; init; }

    public Guid PropertyId { get; init; }

    public Guid ApplicantId { get; init; }

    public decimal ApplicationFee { get; init; }

    public string Currency { get; init; } = default!;
}

