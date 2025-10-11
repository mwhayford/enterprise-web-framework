using Core.Domain.Interfaces;
using Core.Domain.ValueObjects;

namespace Core.Domain.Events;

public record SubscriptionCreatedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid SubscriptionId { get; }
    public Guid UserId { get; }
    public string PlanId { get; }
    public Money Amount { get; }
    public SubscriptionStatus Status { get; }
    public string StripeSubscriptionId { get; }

    public SubscriptionCreatedEvent(
        Guid subscriptionId,
        Guid userId,
        string planId,
        Money amount,
        SubscriptionStatus status,
        string stripeSubscriptionId)
    {
        SubscriptionId = subscriptionId;
        UserId = userId;
        PlanId = planId;
        Amount = amount;
        Status = status;
        StripeSubscriptionId = stripeSubscriptionId;
    }
}
