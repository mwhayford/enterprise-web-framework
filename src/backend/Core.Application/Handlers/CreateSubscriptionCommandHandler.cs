using MediatR;
using Core.Application.Commands;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Application.Mappings;
using Core.Domain.ValueObjects;

namespace Core.Application.Handlers;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
{
    private readonly IPaymentService _paymentService;

    public CreateSubscriptionCommandHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<SubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var amount = Money.Create(request.Amount, request.Currency);
        var payment = await _paymentService.ProcessSubscriptionPaymentAsync(
            request.UserId,
            request.PlanId,
            amount,
            request.PaymentMethodId);

        // Get the subscription that was created by finding the most recent subscription for this user
        var subscriptions = await _paymentService.GetUserSubscriptionsAsync(request.UserId, 1, 1);
        var subscription = subscriptions.FirstOrDefault();
        if (subscription == null)
        {
            throw new InvalidOperationException("Subscription was not created successfully");
        }

        return subscription.ToDto();
    }
}
