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

public class CancelWorkOrderCommandHandler : IRequestHandler<CancelWorkOrderCommand, WorkOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CancelWorkOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<WorkOrderDto> Handle(CancelWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User must be authenticated");

        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken)
            ?? throw new InvalidOperationException("Work order not found");

        // Resident (requested by) or Owner can cancel
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == workOrder.PropertyId, cancellationToken)
            ?? throw new InvalidOperationException("Property not found");

        var canCancel = workOrder.RequestedBy == userId || property.OwnerId == userId;
        if (!canCancel)
        {
            throw new UnauthorizedAccessException("Only the resident who requested or the property owner can cancel work orders");
        }

        workOrder.Cancel(request.Reason);
        await _context.SaveChangesAsync(cancellationToken);

        return WorkOrderMappingHelper.MapToDto(workOrder);
    }
}
