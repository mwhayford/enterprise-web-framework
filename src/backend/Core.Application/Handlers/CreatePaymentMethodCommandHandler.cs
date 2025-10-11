// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Application.Commands;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Core.Application.Mappings;
using MediatR;

namespace Core.Application.Handlers;

public class CreatePaymentMethodCommandHandler : IRequestHandler<CreatePaymentMethodCommand, PaymentMethodDto>
{
    private readonly IPaymentMethodService _paymentMethodService;

    public CreatePaymentMethodCommandHandler(IPaymentMethodService paymentMethodService)
    {
        _paymentMethodService = paymentMethodService;
    }

    public async Task<PaymentMethodDto> Handle(CreatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var paymentMethod = await _paymentMethodService.CreatePaymentMethodAsync(
            request.UserId,
            request.Type,
            request.StripePaymentMethodId,
            request.LastFourDigits,
            request.Brand,
            request.BankName,
            request.IsDefault);

        return paymentMethod.ToDto();
    }
}
