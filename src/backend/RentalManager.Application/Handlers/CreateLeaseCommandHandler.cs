// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.Handlers;

public class CreateLeaseCommandHandler : IRequestHandler<CreateLeaseCommand, LeaseDto>
{
    private readonly IApplicationDbContext _context;

    public CreateLeaseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaseDto> Handle(CreateLeaseCommand request, CancellationToken cancellationToken)
    {
        var monthlyRent = Money.Create(request.LeaseData.MonthlyRent, request.LeaseData.RentCurrency);
        var securityDeposit = Money.Create(request.LeaseData.SecurityDeposit, request.LeaseData.SecurityDepositCurrency);

        var lease = new Lease(
            request.LeaseData.PropertyId,
            request.LeaseData.TenantId,
            request.LeaseData.LandlordId,
            request.LeaseData.StartDate,
            request.LeaseData.EndDate,
            monthlyRent,
            securityDeposit,
            request.LeaseData.PaymentFrequency,
            request.LeaseData.PaymentDayOfMonth,
            request.LeaseData.SpecialTerms);

        if (request.LeaseData.PropertyApplicationId.HasValue)
        {
            lease.AttachApplication(request.LeaseData.PropertyApplicationId.Value);
        }

        _context.Leases.Add(lease);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(lease);
    }

    private static LeaseDto MapToDto(Lease lease)
    {
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

