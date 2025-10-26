// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Queries;

namespace RentalManager.Application.Handlers;

public class GetUserApplicationsQueryHandler : IRequestHandler<GetUserApplicationsQuery, List<PropertyApplicationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserApplicationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PropertyApplicationDto>> Handle(GetUserApplicationsQuery request, CancellationToken cancellationToken)
    {
        var applications = await _context.PropertyApplications
            .Where(a => a.ApplicantId == request.UserId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new PropertyApplicationDto
            {
                Id = a.Id,
                PropertyId = a.PropertyId,
                ApplicantId = a.ApplicantId,
                Status = a.Status,
                ApplicationData = a.ApplicationData,
                ApplicationFee = a.ApplicationFee.Amount,
                ApplicationFeeCurrency = a.ApplicationFee.Currency,
                ApplicationFeePaymentId = a.ApplicationFeePaymentId,
                SubmittedAt = a.SubmittedAt,
                ReviewedAt = a.ReviewedAt,
                ReviewedBy = a.ReviewedBy,
                DecisionNotes = a.DecisionNotes,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return applications;
    }
}

