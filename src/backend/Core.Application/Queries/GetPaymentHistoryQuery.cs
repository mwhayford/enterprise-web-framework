// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Core.Application.DTOs;

namespace Core.Application.Queries;

public record GetPaymentHistoryQuery : IRequest<IEnumerable<PaymentDto>>
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
