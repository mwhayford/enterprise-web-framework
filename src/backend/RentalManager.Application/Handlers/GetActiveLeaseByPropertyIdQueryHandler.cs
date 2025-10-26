// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Queries;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.Handlers;

public class GetActiveLeaseByPropertyIdQueryHandler : IRequestHandler<GetActiveLeaseByPropertyIdQuery, LeaseDto?>
{
    private readonly IApplicationDbContext _context;

    public GetActiveLeaseByPropertyIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaseDto?> Handle(GetActiveLeaseByPropertyIdQuery request, CancellationToken cancellationToken)
    {
        var lease = await _context.Leases
            .Where(l => l.PropertyId == request.PropertyId && l.Status == LeaseStatus.Active)
            .OrderByDescending(l => l.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (lease == null)
        {
            return null;
        }

        return new LeaseDto
        {
            Id = lease.Id,
            PropertyId = lease.PropertyId,
            TenantId = lease.TenantId,
            LandlordId = lease.LandlordId,
            StartDate = lease.StartDate,
            EndDate = lease.EndDate,
            MonthlyRent = lease.MonthlyRent.Amount,
            RentCurrency = lease.MonthlyRent.Currency,
            SecurityDeposit = lease.SecurityDeposit.Amount,
            SecurityDepositCurrency = lease.SecurityDeposit.Currency,
            PaymentFrequency = lease.PaymentFrequency,
            PaymentDayOfMonth = lease.PaymentDayOfMonth,
            Status = lease.Status,
            SpecialTerms = lease.SpecialTerms,
            ActivatedAt = lease.ActivatedAt,
            TerminatedAt = lease.TerminatedAt,
            TerminationReason = lease.TerminationReason,
            PropertyApplicationId = lease.PropertyApplicationId,
            DurationInDays = lease.GetDurationInDays(),
            RemainingDays = lease.GetRemainingDays(),
            IsActive = lease.IsActive,
            IsExpired = lease.IsExpired,
            CreatedAt = lease.CreatedAt,
            UpdatedAt = lease.UpdatedAt
        };
    }
}

