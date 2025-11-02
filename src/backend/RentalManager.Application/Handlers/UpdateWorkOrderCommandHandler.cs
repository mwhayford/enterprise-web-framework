// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Mappings;
using RentalManager.Domain.Entities;

namespace RentalManager.Application.Handlers;

public class UpdateWorkOrderCommandHandler : IRequestHandler<UpdateWorkOrderCommand, WorkOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateWorkOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<WorkOrderDto> Handle(UpdateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User must be authenticated");

        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken)
            ?? throw new InvalidOperationException("Work order not found");

        // Only owner or assigned contractor can update
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == workOrder.PropertyId, cancellationToken)
            ?? throw new InvalidOperationException("Property not found");

        var canUpdate = property.OwnerId == userId || workOrder.AssignedTo == userId;
        if (!canUpdate)
        {
            throw new UnauthorizedAccessException("Only the property owner or assigned contractor can update work orders");
        }

        if (!string.IsNullOrWhiteSpace(request.UpdateData.Title))
        {
            // Title cannot be changed once created, but we can update description
        }

        if (!string.IsNullOrWhiteSpace(request.UpdateData.Description))
        {
            // Description cannot be changed once created
        }

        if (request.UpdateData.EstimatedCost.HasValue)
        {
            workOrder.UpdateEstimatedCost(request.UpdateData.EstimatedCost);
        }

        if (!string.IsNullOrWhiteSpace(request.UpdateData.Notes))
        {
            workOrder.UpdateNotes(request.UpdateData.Notes);
        }

        if (request.UpdateData.Images != null)
        {
            foreach (var imageUrl in request.UpdateData.Images)
            {
                workOrder.AddImage(imageUrl);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return WorkOrderMappingHelper.MapToDto(workOrder);
    }
}


