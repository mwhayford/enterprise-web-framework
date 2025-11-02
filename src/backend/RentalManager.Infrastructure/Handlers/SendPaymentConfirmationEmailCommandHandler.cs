// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using RentalManager.Application.Commands;
using RentalManager.Application.Interfaces;
using RentalManager.Infrastructure.Services;

namespace RentalManager.Infrastructure.Handlers;

public class SendPaymentConfirmationEmailCommandHandler : IRequestHandler<SendPaymentConfirmationEmailCommand>
{
    private readonly IBackgroundJobService _backgroundJobService;

    public SendPaymentConfirmationEmailCommandHandler(IBackgroundJobService backgroundJobService)
    {
        _backgroundJobService = backgroundJobService;
    }

    public async Task Handle(SendPaymentConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        // Enqueue the email sending as a background job
        _backgroundJobService.Enqueue<EmailService>(service =>
            service.SendPaymentConfirmationEmailAsync(request.Email, request.Amount, request.Currency));

        await Task.CompletedTask;
    }
}
