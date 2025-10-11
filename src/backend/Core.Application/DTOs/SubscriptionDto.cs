// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Domain.ValueObjects;

namespace Core.Application.DTOs;

public record SubscriptionDto
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string PlanId { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Currency { get; init; } = "USD";

    public SubscriptionStatus Status { get; init; }

    public DateTime? CurrentPeriodStart { get; init; }

    public DateTime? CurrentPeriodEnd { get; init; }

    public DateTime? TrialStart { get; init; }

    public DateTime? TrialEnd { get; init; }

    public DateTime? CanceledAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public bool IsActive => Status == SubscriptionStatus.Active || Status == SubscriptionStatus.Trialing;
}
