// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Mappings;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.Handlers;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentDto>
{
    private readonly IPaymentService _paymentService;

    public ProcessPaymentCommandHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<PaymentDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty", nameof(request.UserId));
        }

        var amount = Money.Create(request.Amount, request.Currency);
        var payment = await _paymentService.ProcessPaymentAsync(
            request.UserId,
            amount,
            request.PaymentMethodType,
            request.PaymentMethodId,
            request.Description);

        return payment.ToDto();
    }
}
