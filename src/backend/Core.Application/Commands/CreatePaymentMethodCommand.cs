// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Core.Application.DTOs;
using Core.Domain.ValueObjects;

namespace Core.Application.Commands;

public record CreatePaymentMethodCommand : IRequest<PaymentMethodDto>
{
    public Guid UserId { get; init; }
    public PaymentMethodType Type { get; init; }
    public string StripePaymentMethodId { get; init; } = string.Empty;
    public string? LastFourDigits { get; init; }
    public string? Brand { get; init; }
    public string? BankName { get; init; }
    public bool IsDefault { get; init; } = false;
}
