// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Application.DTOs;

public class RenewLeaseDto
{
    public DateTime NewEndDate { get; set; }

    public decimal? NewMonthlyRent { get; set; }

    public string? Currency { get; set; }
}

