// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Core.Application.Commands;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Application.Mappings;
using Core.Domain.ValueObjects;

namespace Core.Application.Handlers;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentDto>
{
    private readonly IPaymentService _paymentService;

    public ProcessPaymentCommandHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<PaymentDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
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
