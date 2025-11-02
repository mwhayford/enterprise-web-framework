// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Queries;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.Handlers;

public class GetApplicationsByOwnerIdQueryHandler : IRequestHandler<GetApplicationsByOwnerIdQuery, List<PropertyApplicationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetApplicationsByOwnerIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PropertyApplicationDto>> Handle(GetApplicationsByOwnerIdQuery request, CancellationToken cancellationToken)
    {
        // Get all properties owned by this owner
        var propertyIds = await _context.Properties
            .Where(p => p.OwnerId == request.OwnerId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        // Get applications for those properties
        var query = _context.PropertyApplications
            .Where(a => propertyIds.Contains(a.PropertyId));

        // Filter by status if provided
        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        var applications = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return applications.Select(MapToDto).ToList();
    }

    private static PropertyApplicationDto MapToDto(Domain.Entities.PropertyApplication application)
    {
        return new PropertyApplicationDto
        {
            Id = application.Id,
            PropertyId = application.PropertyId,
            ApplicantId = application.ApplicantId,
            Status = application.Status,
            ApplicationData = application.ApplicationData,
            ApplicationFee = application.ApplicationFee.Amount,
            ApplicationFeeCurrency = application.ApplicationFee.Currency,
            ApplicationFeePaymentId = application.ApplicationFeePaymentId,
            SubmittedAt = application.SubmittedAt,
            ReviewedAt = application.ReviewedAt,
            ReviewedBy = application.ReviewedBy,
            DecisionNotes = application.DecisionNotes,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt
        };
    }
}