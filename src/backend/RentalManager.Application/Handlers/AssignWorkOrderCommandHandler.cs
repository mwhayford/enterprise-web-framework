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

public class AssignWorkOrderCommandHandler : IRequestHandler<AssignWorkOrderCommand, WorkOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AssignWorkOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<WorkOrderDto> Handle(AssignWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User must be authenticated");

        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken)
            ?? throw new InvalidOperationException("Work order not found");

        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == workOrder.PropertyId, cancellationToken)
            ?? throw new InvalidOperationException("Property not found");

        if (property.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("Only the property owner can assign work orders");
        }

        workOrder.Assign(request.ContractorId);
        await _context.SaveChangesAsync(cancellationToken);

        return WorkOrderMappingHelper.MapToDto(workOrder);
    }
}
