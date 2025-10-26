// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.DTOs;

public record PaymentDto
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public decimal Amount { get; init; }

    public string Currency { get; init; } = "USD";

    public PaymentStatus Status { get; init; }

    public PaymentMethodType PaymentMethodType { get; init; }

    public string? Description { get; init; }

    public string? FailureReason { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? ProcessedAt { get; init; }
}
