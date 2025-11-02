// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Application.DTOs;
using RentalManager.Domain.Entities;

namespace RentalManager.Application.Mappings;

public static class WorkOrderMappingHelper
{
    public static WorkOrderDto MapToDto(WorkOrder workOrder)
    {
        return new WorkOrderDto
        {
            Id = workOrder.Id,
            PropertyId = workOrder.PropertyId,
            LeaseId = workOrder.LeaseId,
            RequestedBy = workOrder.RequestedBy,
            Title = workOrder.Title,
            Description = workOrder.Description,
            Category = workOrder.Category,
            Priority = workOrder.Priority,
            Status = workOrder.Status,
            AssignedTo = workOrder.AssignedTo,
            ApprovedBy = workOrder.ApprovedBy,
            ApprovedAt = workOrder.ApprovedAt,
            AssignedAt = workOrder.AssignedAt,
            StartedAt = workOrder.StartedAt,
            CompletedAt = workOrder.CompletedAt,
            EstimatedCost = workOrder.EstimatedCost,
            ActualCost = workOrder.ActualCost,
            Notes = workOrder.Notes,
            Images = workOrder.Images.ToList(),
            CreatedAt = workOrder.CreatedAt,
            UpdatedAt = workOrder.UpdatedAt
        };
    }
}


