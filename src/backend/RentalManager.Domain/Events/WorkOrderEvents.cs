// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Domain.Events;

public record WorkOrderCreatedEvent : BaseEvent
{
    public Guid WorkOrderId { get; init; }

    public Guid PropertyId { get; init; }

    public Guid LeaseId { get; init; }

    public Guid RequestedBy { get; init; }

    public string Title { get; init; } = default!;

    public WorkOrderCategory Category { get; init; }

    public WorkOrderPriority Priority { get; init; }
}

public record WorkOrderApprovedEvent : BaseEvent
{
    public Guid WorkOrderId { get; init; }

    public Guid PropertyId { get; init; }

    public Guid ApprovedBy { get; init; }

    public DateTime ApprovedAt { get; init; }
}

public record WorkOrderRejectedEvent : BaseEvent
{
    public Guid WorkOrderId { get; init; }

    public Guid PropertyId { get; init; }

    public Guid RejectedBy { get; init; }

    public string? Reason { get; init; }
}

public record WorkOrderAssignedEvent : BaseEvent
{
    public Guid WorkOrderId { get; init; }

    public Guid PropertyId { get; init; }

    public Guid AssignedTo { get; init; }

    public DateTime AssignedAt { get; init; }
}

public record WorkOrderStartedEvent : BaseEvent
{
    public Guid WorkOrderId { get; init; }

    public Guid PropertyId { get; init; }

    public Guid AssignedTo { get; init; }

    public DateTime StartedAt { get; init; }
}

public record WorkOrderCompletedEvent : BaseEvent
{
    public Guid WorkOrderId { get; init; }

    public Guid PropertyId { get; init; }

    public Guid AssignedTo { get; init; }

    public DateTime CompletedAt { get; init; }

    public decimal? ActualCost { get; init; }
}

public record WorkOrderCancelledEvent : BaseEvent
{
    public Guid WorkOrderId { get; init; }

    public Guid PropertyId { get; init; }

    public string? Reason { get; init; }
}

