// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.Events;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Domain.Entities;

public class Payment : BaseEntity
{
    public Payment(
        Guid userId,
        Money amount,
        PaymentMethodType paymentMethodType,
        string? description = null)
        : base()
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        }

        ArgumentNullException.ThrowIfNull(amount);

        UserId = userId;
        Amount = amount;
        PaymentMethodType = paymentMethodType;
        Description = description;
        Status = PaymentStatus.Pending;
    }

    private Payment()
    {
        Amount = default!;
    } // For EF Core

    public Guid UserId { get; private set; }

    public Money Amount { get; private set; }

    public PaymentStatus Status { get; private set; }

    public PaymentMethodType PaymentMethodType { get; private set; }

    public string? StripePaymentIntentId { get; private set; }

    public string? StripeChargeId { get; private set; }

    public string? Description { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    public void SetStripePaymentIntentId(string stripePaymentIntentId)
    {
        StripePaymentIntentId = stripePaymentIntentId;
        UpdateTimestamp();
    }

    public void Process(string stripeChargeId)
    {
        Status = PaymentStatus.Processing;
        StripeChargeId = stripeChargeId;
        ProcessedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Succeed()
    {
        Status = PaymentStatus.Succeeded;
        ProcessedAt = DateTime.UtcNow;
        UpdateTimestamp();

        AddDomainEvent(new PaymentProcessedEvent
        {
            PaymentId = Id,
            UserId = UserId,
            Amount = Amount.Amount,
            Currency = Amount.Currency,
            Status = Status.ToString(),
            PaymentMethodId = StripePaymentIntentId ?? string.Empty
        });
    }

    public void Fail(string failureReason)
    {
        if (string.IsNullOrWhiteSpace(failureReason))
        {
            throw new ArgumentException("Failure reason cannot be null or empty", nameof(failureReason));
        }

        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
        ProcessedAt = DateTime.UtcNow;
        UpdateTimestamp();

        AddDomainEvent(new PaymentFailedEvent
        {
            PaymentId = Id,
            UserId = UserId,
            Amount = Amount.Amount,
            Currency = Amount.Currency,
            FailureReason = failureReason
        });
    }

    public void Cancel()
    {
        Status = PaymentStatus.Cancelled;
        ProcessedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Refund()
    {
        Status = PaymentStatus.Refunded;
        ProcessedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void PartialRefund()
    {
        Status = PaymentStatus.PartiallyRefunded;
        ProcessedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }
}
