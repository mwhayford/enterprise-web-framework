// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Mappings;
using RentalManager.Application.Queries;

namespace RentalManager.Application.Handlers;

public class GetWorkOrdersByOwnerIdQueryHandler : IRequestHandler<GetWorkOrdersByOwnerIdQuery, List<WorkOrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkOrdersByOwnerIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkOrderDto>> Handle(GetWorkOrdersByOwnerIdQuery request, CancellationToken cancellationToken)
    {
        // Get all properties owned by this owner, then get work orders for those properties
        var propertyIds = await _context.Properties
            .Where(p => p.OwnerId == request.OwnerId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var workOrders = await _context.WorkOrders
            .Where(w => propertyIds.Contains(w.PropertyId))
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        return workOrders.Select(WorkOrderMappingHelper.MapToDto).ToList();
    }
}


