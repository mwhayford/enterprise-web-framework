// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.DTOs;

public class LeaseDto
{
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }

    public Guid TenantId { get; set; }

    public Guid LandlordId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal MonthlyRent { get; set; }

    public string RentCurrency { get; set; } = default!;

    public decimal SecurityDeposit { get; set; }

    public string SecurityDepositCurrency { get; set; } = default!;

    public PaymentFrequency PaymentFrequency { get; set; }

    public int PaymentDayOfMonth { get; set; }

    public LeaseStatus Status { get; set; }

    public string? SpecialTerms { get; set; }

    public DateTime? ActivatedAt { get; set; }

    public DateTime? TerminatedAt { get; set; }

    public string? TerminationReason { get; set; }

    public Guid? PropertyApplicationId { get; set; }

    public int DurationInDays { get; set; }

    public int RemainingDays { get; set; }

    public bool IsActive { get; set; }

    public bool IsExpired { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
