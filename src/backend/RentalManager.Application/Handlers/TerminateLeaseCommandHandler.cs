// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;

namespace RentalManager.Application.Handlers;

public class TerminateLeaseCommandHandler : IRequestHandler<TerminateLeaseCommand, LeaseDto>
{
    private readonly IApplicationDbContext _context;

    public TerminateLeaseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaseDto> Handle(TerminateLeaseCommand request, CancellationToken cancellationToken)
    {
        var lease = await _context.Leases
            .FirstOrDefaultAsync(l => l.Id == request.LeaseId, cancellationToken);

        if (lease == null)
        {
            throw new KeyNotFoundException($"Lease with ID {request.LeaseId} not found");
        }

        lease.Terminate(request.Reason);
        await _context.SaveChangesAsync(cancellationToken);

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
