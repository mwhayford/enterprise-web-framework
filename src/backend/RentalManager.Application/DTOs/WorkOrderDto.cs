// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.DTOs;

public class WorkOrderDto
{
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }

    public Guid LeaseId { get; set; }

    public Guid RequestedBy { get; set; }

    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public WorkOrderCategory Category { get; set; }

    public WorkOrderPriority Priority { get; set; }

    public WorkOrderStatus Status { get; set; }

    public Guid? AssignedTo { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public decimal? EstimatedCost { get; set; }

    public decimal? ActualCost { get; set; }

    public string? Notes { get; set; }

    public List<string> Images { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}