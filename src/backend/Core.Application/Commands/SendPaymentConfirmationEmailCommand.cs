// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;

namespace Core.Application.Commands;

public record SendPaymentConfirmationEmailCommand : IRequest
{
    public string Email { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}
