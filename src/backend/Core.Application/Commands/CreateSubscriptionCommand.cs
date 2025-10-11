using MediatR;
using Core.Application.DTOs;

namespace Core.Application.Commands;

public record CreateSubscriptionCommand : IRequest<SubscriptionDto>
{
    public Guid UserId { get; init; }
    public string PlanId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string? PaymentMethodId { get; init; }
    public DateTime? TrialStart { get; init; }
    public DateTime? TrialEnd { get; init; }
}
