// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Mappings;
using RentalManager.Application.Queries;

namespace RentalManager.Application.Handlers;

public class GetWorkOrdersByContractorIdQueryHandler : IRequestHandler<GetWorkOrdersByContractorIdQuery, List<WorkOrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkOrdersByContractorIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkOrderDto>> Handle(GetWorkOrdersByContractorIdQuery request, CancellationToken cancellationToken)
    {
        var workOrders = await _context.WorkOrders
            .Where(w => w.AssignedTo == request.ContractorId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        return workOrders.Select(WorkOrderMappingHelper.MapToDto).ToList();
    }
}
