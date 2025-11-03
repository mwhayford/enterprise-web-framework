// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Application.DTOs;

public class UpdateWorkOrderDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal? EstimatedCost { get; set; }

    public string? Notes { get; set; }

    public List<string>? Images { get; set; }
}
