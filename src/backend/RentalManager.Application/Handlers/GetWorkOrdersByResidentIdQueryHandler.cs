// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Mappings;
using RentalManager.Application.Queries;

namespace RentalManager.Application.Handlers;

public class GetWorkOrdersByResidentIdQueryHandler : IRequestHandler<GetWorkOrdersByResidentIdQuery, List<WorkOrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkOrdersByResidentIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkOrderDto>> Handle(GetWorkOrdersByResidentIdQuery request, CancellationToken cancellationToken)
    {
        var workOrders = await _context.WorkOrders
            .Where(w => w.RequestedBy == request.ResidentId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        return workOrders.Select(WorkOrderMappingHelper.MapToDto).ToList();
    }
}


