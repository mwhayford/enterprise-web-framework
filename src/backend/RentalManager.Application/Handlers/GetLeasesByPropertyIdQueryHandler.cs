// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Queries;

namespace RentalManager.Application.Handlers;

public class GetLeasesByPropertyIdQueryHandler : IRequestHandler<GetLeasesByPropertyIdQuery, List<LeaseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLeasesByPropertyIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaseDto>> Handle(GetLeasesByPropertyIdQuery request, CancellationToken cancellationToken)
    {
        var leases = await _context.Leases
            .Where(l => l.PropertyId == request.PropertyId)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync(cancellationToken);

        return leases.Select(lease => new LeaseDto
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
        }).ToList();
    }
}
