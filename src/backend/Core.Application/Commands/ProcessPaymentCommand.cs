// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Core.Application.DTOs;
using Core.Domain.ValueObjects;

namespace Core.Application.Commands;

public record ProcessPaymentCommand : IRequest<PaymentDto>
{
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public PaymentMethodType PaymentMethodType { get; init; }
    public string? PaymentMethodId { get; init; }
    public string? Description { get; init; }
}
