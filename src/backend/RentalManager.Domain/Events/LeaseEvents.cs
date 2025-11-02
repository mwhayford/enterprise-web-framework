// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Domain.Events;

public record LeaseCreatedEvent : BaseEvent
{
    public Guid LeaseId { get; init; }

    public Guid PropertyId { get; init; }

    public Guid TenantId { get; init; }

    public Guid LandlordId { get; init; }

    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public decimal MonthlyRent { get; init; }

    public string Currency { get; init; } = default!;
}

public record LeaseActivatedEvent : BaseEvent
{
    public Guid LeaseId { get; init; }

    public Guid PropertyId { get; init; }

    public Guid TenantId { get; init; }

    public DateTime ActivatedAt { get; init; }
}

public record LeaseTerminatedEvent : BaseEvent
{
    public Guid LeaseId { get; init; }

    public Guid PropertyId { get; init; }

    public Guid TenantId { get; init; }

    public DateTime TerminatedAt { get; init; }

    public string Reason { get; init; } = default!;
}

public record LeaseExpiredEvent : BaseEvent
{
    public Guid LeaseId { get; init; }

    public Guid PropertyId { get; init; }

    public Guid TenantId { get; init; }

    public DateTime EndDate { get; init; }
}

public record LeaseRenewedEvent : BaseEvent
{
    public Guid OriginalLeaseId { get; init; }

    public Guid NewLeaseId { get; init; }

    public Guid PropertyId { get; init; }

    public Guid TenantId { get; init; }

    public DateTime NewEndDate { get; init; }
}

public record LeaseRentUpdatedEvent : BaseEvent
{
    public Guid LeaseId { get; init; }

    public Guid PropertyId { get; init; }

    public decimal OldRent { get; init; }

    public decimal NewRent { get; init; }

    public string Currency { get; init; } = default!;
}
