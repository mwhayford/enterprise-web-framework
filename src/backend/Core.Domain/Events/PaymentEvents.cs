// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace Core.Domain.Events;

public record PaymentProcessedEvent : BaseEvent
{
    public Guid PaymentId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string PaymentMethodId { get; init; } = string.Empty;
}

public record PaymentFailedEvent : BaseEvent
{
    public Guid PaymentId { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string FailureReason { get; init; } = string.Empty;
}

public record SubscriptionCreatedEvent : BaseEvent
{
    public Guid SubscriptionId { get; init; }
    public Guid UserId { get; init; }
    public string PlanId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public record SubscriptionCancelledEvent : BaseEvent
{
    public Guid SubscriptionId { get; init; }
    public Guid UserId { get; init; }
    public string PlanId { get; init; } = string.Empty;
    public DateTime CancelledAt { get; init; }
}
