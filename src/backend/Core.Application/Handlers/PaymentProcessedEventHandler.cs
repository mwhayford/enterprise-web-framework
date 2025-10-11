// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.Extensions.Logging;
using Core.Domain.Events;

namespace Core.Application.Handlers;

public class PaymentProcessedEventHandler : INotificationHandler<PaymentProcessedEvent>
{
    private readonly ILogger<PaymentProcessedEventHandler> _logger;

    public PaymentProcessedEventHandler(ILogger<PaymentProcessedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PaymentProcessedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling PaymentProcessedEvent for payment {PaymentId} of {Amount} {Currency}", 
            notification.PaymentId, notification.Amount, notification.Currency);

        // Here you can add business logic like:
        // - Send payment confirmation email
        // - Update user account balance
        // - Trigger fulfillment process
        // - Update analytics
        // - Index payment in search
        // - Send notification to mobile app

        await Task.CompletedTask;
    }
}
