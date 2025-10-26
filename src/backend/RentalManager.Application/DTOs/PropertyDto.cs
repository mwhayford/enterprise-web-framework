// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.DTOs;

public class PropertyDto
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Street { get; set; } = default!;

    public string? Unit { get; set; }

    public string City { get; set; } = default!;

    public string State { get; set; } = default!;

    public string ZipCode { get; set; } = default!;

    public string Country { get; set; } = default!;

    public PropertyType PropertyType { get; set; }

    public int Bedrooms { get; set; }

    public int Bathrooms { get; set; }

    public decimal SquareFeet { get; set; }

    public decimal MonthlyRent { get; set; }

    public string RentCurrency { get; set; } = default!;

    public decimal SecurityDeposit { get; set; }

    public string SecurityDepositCurrency { get; set; } = default!;

    public DateTime AvailableDate { get; set; }

    public PropertyStatus Status { get; set; }

    public string Description { get; set; } = default!;

    public decimal? ApplicationFee { get; set; }

    public string? ApplicationFeeCurrency { get; set; }

    public List<string> Amenities { get; set; } = new();

    public List<string> Images { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

