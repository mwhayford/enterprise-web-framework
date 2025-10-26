// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Queries;

namespace RentalManager.Application.Handlers;

public class GetApplicationByIdQueryHandler : IRequestHandler<GetApplicationByIdQuery, PropertyApplicationDto?>
{
    private readonly IApplicationDbContext _context;

    public GetApplicationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyApplicationDto?> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var application = await _context.PropertyApplications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application == null)
        {
            return null;
        }

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

