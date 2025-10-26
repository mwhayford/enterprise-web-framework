// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.DTOs;

public class PropertyApplicationDto
{
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }

    public Guid ApplicantId { get; set; }

    public ApplicationStatus Status { get; set; }

    public string ApplicationData { get; set; } = default!;

    public decimal ApplicationFee { get; set; }

    public string ApplicationFeeCurrency { get; set; } = default!;

    public Guid? ApplicationFeePaymentId { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewedBy { get; set; }

    public string? DecisionNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

