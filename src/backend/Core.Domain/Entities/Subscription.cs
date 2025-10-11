// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Domain.Events;
using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; private set; }
    public string PlanId { get; private set; }
    public Money Amount { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public string? StripeSubscriptionId { get; private set; }
    public string? StripeCustomerId { get; private set; }
    public DateTime? CurrentPeriodStart { get; private set; }
    public DateTime? CurrentPeriodEnd { get; private set; }
    public DateTime? CanceledAt { get; private set; }
    public DateTime? TrialStart { get; private set; }
    public DateTime? TrialEnd { get; private set; }

    private Subscription() { } // For EF Core

    public Subscription(
        Guid userId,
        string planId,
        Money amount,
        string? stripeCustomerId = null)
        : base()
    {
        UserId = userId;
        PlanId = planId;
        Amount = amount;
        StripeCustomerId = stripeCustomerId;
        Status = SubscriptionStatus.Incomplete;
    }

    public void SetStripeSubscriptionId(string stripeSubscriptionId)
    {
        StripeSubscriptionId = stripeSubscriptionId;
        UpdateTimestamp();
    }

    public void Activate(DateTime currentPeriodStart, DateTime currentPeriodEnd)
    {
        Status = SubscriptionStatus.Active;
        CurrentPeriodStart = currentPeriodStart;
        CurrentPeriodEnd = currentPeriodEnd;
        UpdateTimestamp();

        AddDomainEvent(new SubscriptionCreatedEvent
        {
            SubscriptionId = Id,
            UserId = UserId,
            PlanId = PlanId,
            Amount = Amount.Amount,
            Currency = Amount.Currency,
            Status = Status.ToString()
        });
    }

    public void StartTrial(DateTime trialStart, DateTime trialEnd)
    {
        Status = SubscriptionStatus.Trialing;
        TrialStart = trialStart;
        TrialEnd = trialEnd;
        UpdateTimestamp();
    }

    public void UpdatePeriod(DateTime currentPeriodStart, DateTime currentPeriodEnd)
    {
        CurrentPeriodStart = currentPeriodStart;
        CurrentPeriodEnd = currentPeriodEnd;
        UpdateTimestamp();
    }

    public void MarkPastDue()
    {
        Status = SubscriptionStatus.PastDue;
        UpdateTimestamp();
    }

    public void Cancel(DateTime canceledAt)
    {
        Status = SubscriptionStatus.Canceled;
        CanceledAt = canceledAt;
        UpdateTimestamp();
    }

    public void MarkUnpaid()
    {
        Status = SubscriptionStatus.Unpaid;
        UpdateTimestamp();
    }

    public void Pause()
    {
        Status = SubscriptionStatus.Paused;
        UpdateTimestamp();
    }

    public void Resume()
    {
        Status = SubscriptionStatus.Active;
        UpdateTimestamp();
    }

    public void UpdatePlan(string planId, Money amount)
    {
        PlanId = planId;
        Amount = amount;
        UpdateTimestamp();
    }

    public bool IsActive => Status == SubscriptionStatus.Active || Status == SubscriptionStatus.Trialing;
    public bool IsCanceled => Status == SubscriptionStatus.Canceled;
    public bool IsPastDue => Status == SubscriptionStatus.PastDue;
}
