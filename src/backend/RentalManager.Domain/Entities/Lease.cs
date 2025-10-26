// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.Events;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Domain.Entities;

public class Lease : BaseEntity
{
    public Lease(
        Guid propertyId,
        Guid tenantId,
        Guid landlordId,
        DateTime startDate,
        DateTime endDate,
        Money monthlyRent,
        Money securityDeposit,
        PaymentFrequency paymentFrequency,
        int paymentDayOfMonth,
        string? specialTerms = null)
        : base()
    {
        if (propertyId == Guid.Empty)
        {
            throw new ArgumentException("Property ID cannot be empty", nameof(propertyId));
        }

        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
        }

        if (landlordId == Guid.Empty)
        {
            throw new ArgumentException("Landlord ID cannot be empty", nameof(landlordId));
        }

        if (endDate <= startDate)
        {
            throw new ArgumentException("End date must be after start date", nameof(endDate));
        }

        ArgumentNullException.ThrowIfNull(monthlyRent);
        ArgumentNullException.ThrowIfNull(securityDeposit);

        if (paymentDayOfMonth < 1 || paymentDayOfMonth > 28)
        {
            throw new ArgumentException("Payment day of month must be between 1 and 28", nameof(paymentDayOfMonth));
        }

        PropertyId = propertyId;
        TenantId = tenantId;
        LandlordId = landlordId;
        StartDate = startDate;
        EndDate = endDate;
        MonthlyRent = monthlyRent;
        SecurityDeposit = securityDeposit;
        PaymentFrequency = paymentFrequency;
        PaymentDayOfMonth = paymentDayOfMonth;
        SpecialTerms = specialTerms;
        Status = LeaseStatus.Draft;

        AddDomainEvent(new LeaseCreatedEvent
        {
            LeaseId = Id,
            PropertyId = propertyId,
            TenantId = tenantId,
            LandlordId = landlordId,
            StartDate = startDate,
            EndDate = endDate,
            MonthlyRent = monthlyRent.Amount,
            Currency = monthlyRent.Currency
        });
    }

    private Lease()
    {
        MonthlyRent = default!;
        SecurityDeposit = default!;
    } // For EF Core

    public Guid PropertyId { get; private set; }

    public Guid TenantId { get; private set; }

    public Guid LandlordId { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public Money MonthlyRent { get; private set; }

    public Money SecurityDeposit { get; private set; }

    public PaymentFrequency PaymentFrequency { get; private set; }

    public int PaymentDayOfMonth { get; private set; }

    public LeaseStatus Status { get; private set; }

    public string? SpecialTerms { get; private set; }

    public DateTime? ActivatedAt { get; private set; }

    public DateTime? TerminatedAt { get; private set; }

    public string? TerminationReason { get; private set; }

    public Guid? PropertyApplicationId { get; private set; }

    public bool IsActive => Status == LeaseStatus.Active;

    public bool IsExpired => Status == LeaseStatus.Expired || (Status == LeaseStatus.Active && DateTime.UtcNow > EndDate);

    public bool CanBeActivated => Status == LeaseStatus.Draft && DateTime.UtcNow >= StartDate.AddDays(-30);

    public bool CanBeTerminated => Status == LeaseStatus.Active;

    public bool CanBeRenewed => (Status == LeaseStatus.Active || Status == LeaseStatus.Expired) && DateTime.UtcNow >= EndDate.AddDays(-60);

    public void Activate()
    {
        if (!CanBeActivated)
        {
            throw new InvalidOperationException("Lease cannot be activated in its current state");
        }

        Status = LeaseStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        UpdateTimestamp();

        AddDomainEvent(new LeaseActivatedEvent
        {
            LeaseId = Id,
            PropertyId = PropertyId,
            TenantId = TenantId,
            ActivatedAt = ActivatedAt.Value
        });
    }

    public void Terminate(string reason)
    {
        if (!CanBeTerminated)
        {
            throw new InvalidOperationException("Lease cannot be terminated in its current state");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Termination reason is required", nameof(reason));
        }

        Status = LeaseStatus.Terminated;
        TerminatedAt = DateTime.UtcNow;
        TerminationReason = reason;
        UpdateTimestamp();

        AddDomainEvent(new LeaseTerminatedEvent
        {
            LeaseId = Id,
            PropertyId = PropertyId,
            TenantId = TenantId,
            TerminatedAt = TerminatedAt.Value,
            Reason = reason
        });
    }

    public void MarkAsExpired()
    {
        if (Status != LeaseStatus.Active)
        {
            throw new InvalidOperationException("Only active leases can be marked as expired");
        }

        Status = LeaseStatus.Expired;
        UpdateTimestamp();

        AddDomainEvent(new LeaseExpiredEvent
        {
            LeaseId = Id,
            PropertyId = PropertyId,
            TenantId = TenantId,
            EndDate = EndDate
        });
    }

    public Lease Renew(DateTime newEndDate, Money? newMonthlyRent = null)
    {
        if (!CanBeRenewed)
        {
            throw new InvalidOperationException("Lease cannot be renewed in its current state");
        }

        if (newEndDate <= EndDate)
        {
            throw new ArgumentException("New end date must be after current end date", nameof(newEndDate));
        }

        // Mark current lease as renewed
        Status = LeaseStatus.Renewed;
        UpdateTimestamp();

        // Create new lease
        var renewedLease = new Lease(
            PropertyId,
            TenantId,
            LandlordId,
            EndDate.AddDays(1),
            newEndDate,
            newMonthlyRent ?? MonthlyRent,
            SecurityDeposit,
            PaymentFrequency,
            PaymentDayOfMonth,
            SpecialTerms);

        AddDomainEvent(new LeaseRenewedEvent
        {
            OriginalLeaseId = Id,
            NewLeaseId = renewedLease.Id,
            PropertyId = PropertyId,
            TenantId = TenantId,
            NewEndDate = newEndDate
        });

        return renewedLease;
    }

    public void UpdateRent(Money newMonthlyRent)
    {
        if (Status != LeaseStatus.Active && Status != LeaseStatus.Draft)
        {
            throw new InvalidOperationException("Rent can only be updated for active or draft leases");
        }

        ArgumentNullException.ThrowIfNull(newMonthlyRent);

        var oldRent = MonthlyRent;
        MonthlyRent = newMonthlyRent;
        UpdateTimestamp();

        AddDomainEvent(new LeaseRentUpdatedEvent
        {
            LeaseId = Id,
            PropertyId = PropertyId,
            OldRent = oldRent.Amount,
            NewRent = newMonthlyRent.Amount,
            Currency = newMonthlyRent.Currency
        });
    }

    public void AttachApplication(Guid applicationId)
    {
        if (applicationId == Guid.Empty)
        {
            throw new ArgumentException("Application ID cannot be empty", nameof(applicationId));
        }

        PropertyApplicationId = applicationId;
        UpdateTimestamp();
    }

    public void UpdateSpecialTerms(string? specialTerms)
    {
        SpecialTerms = specialTerms;
        UpdateTimestamp();
    }

    public int GetDurationInDays()
    {
        return (EndDate - StartDate).Days;
    }

    public int GetRemainingDays()
    {
        if (Status != LeaseStatus.Active)
        {
            return 0;
        }

        var remaining = (EndDate - DateTime.UtcNow).Days;
        return remaining > 0 ? remaining : 0;
    }
}


