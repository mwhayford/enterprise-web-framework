// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.DTOs;

public class CreateLeaseDto
{
    public Guid PropertyId { get; set; }

    public Guid TenantId { get; set; }

    public Guid LandlordId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal MonthlyRent { get; set; }

    public string RentCurrency { get; set; } = "USD";

    public decimal SecurityDeposit { get; set; }

    public string SecurityDepositCurrency { get; set; } = "USD";

    public PaymentFrequency PaymentFrequency { get; set; } = PaymentFrequency.Monthly;

    public int PaymentDayOfMonth { get; set; } = 1;

    public string? SpecialTerms { get; set; }

    public Guid? PropertyApplicationId { get; set; }
}
