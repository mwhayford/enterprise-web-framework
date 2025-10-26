// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.DTOs;

public class PropertyListDto
{
    public Guid Id { get; set; }

    public string Address { get; set; } = default!;

    public PropertyType PropertyType { get; set; }

    public int Bedrooms { get; set; }

    public int Bathrooms { get; set; }

    public decimal SquareFeet { get; set; }

    public decimal MonthlyRent { get; set; }

    public string RentCurrency { get; set; } = default!;

    public DateTime AvailableDate { get; set; }

    public PropertyStatus Status { get; set; }

    public string? ThumbnailImage { get; set; }

    public decimal? ApplicationFee { get; set; }
}

