using Core.Domain.Interfaces;
using Core.Domain.ValueObjects;

namespace Core.Domain.Events;

public record PaymentProcessedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid PaymentId { get; }
    public Guid UserId { get; }
    public Money Amount { get; }
    public PaymentStatus Status { get; }
    public string StripePaymentIntentId { get; }

    public PaymentProcessedEvent(
        Guid paymentId,
        Guid userId,
        Money amount,
        PaymentStatus status,
        string stripePaymentIntentId)
    {
        PaymentId = paymentId;
        UserId = userId;
        Amount = amount;
        Status = status;
        StripePaymentIntentId = stripePaymentIntentId;
    }
}
