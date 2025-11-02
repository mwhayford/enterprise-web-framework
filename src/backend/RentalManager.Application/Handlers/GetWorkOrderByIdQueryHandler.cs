// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Mappings;
using RentalManager.Application.Queries;

namespace RentalManager.Application.Handlers;

public class GetWorkOrderByIdQueryHandler : IRequestHandler<GetWorkOrderByIdQuery, WorkOrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetWorkOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkOrderDto?> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == request.WorkOrderId, cancellationToken);

        if (workOrder == null)
        {
            return null;
        }

        return WorkOrderMappingHelper.MapToDto(workOrder);
    }
}


