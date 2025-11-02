// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.Handlers;

public class RenewLeaseCommandHandler : IRequestHandler<RenewLeaseCommand, LeaseDto>
{
    private readonly IApplicationDbContext _context;

    public RenewLeaseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaseDto> Handle(RenewLeaseCommand request, CancellationToken cancellationToken)
    {
        var lease = await _context.Leases
            .FirstOrDefaultAsync(l => l.Id == request.LeaseId, cancellationToken);

        if (lease == null)
        {
            throw new KeyNotFoundException($"Lease with ID {request.LeaseId} not found");
        }

        Money? newMonthlyRent = null;
        if (request.RenewalData.NewMonthlyRent.HasValue)
        {
            var currency = request.RenewalData.Currency ?? lease.MonthlyRent.Currency;
            newMonthlyRent = Money.Create(request.RenewalData.NewMonthlyRent.Value, currency);
        }

        var renewedLease = lease.Renew(request.RenewalData.NewEndDate, newMonthlyRent);

        _context.Leases.Add(renewedLease);
        await _context.SaveChangesAsync(cancellationToken);

        return new LeaseDto
        {
            Id = renewedLease.Id,
            PropertyId = renewedLease.PropertyId,
            TenantId = renewedLease.TenantId,
            LandlordId = renewedLease.LandlordId,
            StartDate = renewedLease.StartDate,
            EndDate = renewedLease.EndDate,
            MonthlyRent = renewedLease.MonthlyRent.Amount,
            RentCurrency = renewedLease.MonthlyRent.Currency,
            SecurityDeposit = renewedLease.SecurityDeposit.Amount,
            SecurityDepositCurrency = renewedLease.SecurityDeposit.Currency,
            PaymentFrequency = renewedLease.PaymentFrequency,
            PaymentDayOfMonth = renewedLease.PaymentDayOfMonth,
            Status = renewedLease.Status,
            SpecialTerms = renewedLease.SpecialTerms,
            ActivatedAt = renewedLease.ActivatedAt,
            TerminatedAt = renewedLease.TerminatedAt,
            TerminationReason = renewedLease.TerminationReason,
            PropertyApplicationId = renewedLease.PropertyApplicationId,
            DurationInDays = renewedLease.GetDurationInDays(),
            RemainingDays = renewedLease.GetRemainingDays(),
            IsActive = renewedLease.IsActive,
            IsExpired = renewedLease.IsExpired,
            CreatedAt = renewedLease.CreatedAt,
            UpdatedAt = renewedLease.UpdatedAt
        };
    }
}
