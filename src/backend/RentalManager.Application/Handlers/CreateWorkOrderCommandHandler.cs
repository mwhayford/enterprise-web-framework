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

public class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, WorkOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateWorkOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<WorkOrderDto> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User must be authenticated");

        // Verify lease exists and user is the tenant
        var lease = await _context.Leases
            .FirstOrDefaultAsync(l => l.Id == request.WorkOrderData.LeaseId, cancellationToken)
            ?? throw new InvalidOperationException("Lease not found");

        if (lease.TenantId != userId)
        {
            throw new UnauthorizedAccessException("Only the tenant can create work orders for this lease");
        }

        if (!lease.IsActive)
        {
            throw new InvalidOperationException("Work orders can only be created for active leases");
        }

        var workOrder = new WorkOrder(
            request.WorkOrderData.PropertyId,
            request.WorkOrderData.LeaseId,
            userId,
            request.WorkOrderData.Title,
            request.WorkOrderData.Description,
            request.WorkOrderData.Category,
            request.WorkOrderData.Priority);

        if (request.WorkOrderData.Images != null)
        {
            foreach (var imageUrl in request.WorkOrderData.Images)
            {
                workOrder.AddImage(imageUrl);
            }
        }

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync(cancellationToken);

        return WorkOrderMappingHelper.MapToDto(workOrder);
    }
}