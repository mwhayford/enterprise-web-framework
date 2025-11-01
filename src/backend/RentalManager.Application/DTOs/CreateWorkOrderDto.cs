// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.DTOs;

public class CreateWorkOrderDto
{
    public Guid PropertyId { get; set; }

    public Guid LeaseId { get; set; }

    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public WorkOrderCategory Category { get; set; }

    public WorkOrderPriority Priority { get; set; }

    public List<string>? Images { get; set; }
}
