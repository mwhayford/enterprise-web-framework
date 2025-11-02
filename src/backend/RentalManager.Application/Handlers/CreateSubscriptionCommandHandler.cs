// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Mappings;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.Handlers;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
{
    private readonly IPaymentService _paymentService;

    public CreateSubscriptionCommandHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<SubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty", nameof(request.UserId));
        }

        if (string.IsNullOrWhiteSpace(request.PlanId))
        {
            throw new ArgumentException("Plan ID cannot be null or empty", nameof(request.PlanId));
        }

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
            throw new InvalidOperationException("Created subscription not found in user subscriptions");
        }

        return subscription.ToDto();
    }
}
