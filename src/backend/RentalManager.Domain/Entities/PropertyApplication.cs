// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.Events;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Domain.Entities;

public class PropertyApplication : BaseEntity
{
    public PropertyApplication(
        Guid propertyId,
        Guid applicantId,
        string applicationData,
        Money applicationFee)
        : base()
    {
        if (propertyId == Guid.Empty)
        {
            throw new ArgumentException("Property ID cannot be empty", nameof(propertyId));
        }

        if (applicantId == Guid.Empty)
        {
            throw new ArgumentException("Applicant ID cannot be empty", nameof(applicantId));
        }

        ArgumentNullException.ThrowIfNull(applicationData);
        ArgumentNullException.ThrowIfNull(applicationFee);

        PropertyId = propertyId;
        ApplicantId = applicantId;
        ApplicationData = applicationData;
        ApplicationFee = applicationFee;
        Status = ApplicationStatus.Pending;
    }

    private PropertyApplication()
    {
        ApplicationData = default!;
        ApplicationFee = default!;
    } // For EF Core

    public Guid PropertyId { get; private set; }

    public Guid ApplicantId { get; private set; }

    public ApplicationStatus Status { get; private set; }

    public string ApplicationData { get; private set; }

    public Money ApplicationFee { get; private set; }

    public Guid? ApplicationFeePaymentId { get; private set; }

    public DateTime? SubmittedAt { get; private set; }

    public DateTime? ReviewedAt { get; private set; }

    public Guid? ReviewedBy { get; private set; }

    public string? DecisionNotes { get; private set; }

    public bool IsSubmitted => SubmittedAt.HasValue;

    public bool IsReviewed => ReviewedAt.HasValue;

    public bool IsApproved => Status == ApplicationStatus.Approved;

    public bool IsRejected => Status == ApplicationStatus.Rejected;

    public bool IsWithdrawn => Status == ApplicationStatus.Withdrawn;

    public void Submit()
    {
        if (SubmittedAt.HasValue)
        {
            throw new InvalidOperationException("Application has already been submitted");
        }

        SubmittedAt = DateTime.UtcNow;
        Status = ApplicationStatus.Pending;
        UpdateTimestamp();

        AddDomainEvent(new PropertyApplicationSubmittedEvent
        {
            ApplicationId = Id,
            PropertyId = PropertyId,
            ApplicantId = ApplicantId,
            ApplicationFee = ApplicationFee.Amount,
            Currency = ApplicationFee.Currency
        });
    }

    public void AttachPayment(Guid paymentId)
    {
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("Payment ID cannot be empty", nameof(paymentId));
        }

        ApplicationFeePaymentId = paymentId;
        UpdateTimestamp();
    }

    public void Review(Guid reviewedBy)
    {
        if (reviewedBy == Guid.Empty)
        {
            throw new ArgumentException("Reviewer ID cannot be empty", nameof(reviewedBy));
        }

        if (!SubmittedAt.HasValue)
        {
            throw new InvalidOperationException("Application must be submitted before it can be reviewed");
        }

        if (Status == ApplicationStatus.Withdrawn)
        {
            throw new InvalidOperationException("Cannot review a withdrawn application");
        }

        Status = ApplicationStatus.UnderReview;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewedBy;
        UpdateTimestamp();
    }

    public void Approve(Guid reviewedBy, string? decisionNotes = null)
    {
        if (reviewedBy == Guid.Empty)
        {
            throw new ArgumentException("Reviewer ID cannot be empty", nameof(reviewedBy));
        }

        if (!SubmittedAt.HasValue)
        {
            throw new InvalidOperationException("Application must be submitted before it can be approved");
        }

        if (Status == ApplicationStatus.Withdrawn)
        {
            throw new InvalidOperationException("Cannot approve a withdrawn application");
        }

        Status = ApplicationStatus.Approved;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewedBy;
        DecisionNotes = decisionNotes;
        UpdateTimestamp();

        AddDomainEvent(new PropertyApplicationApprovedEvent
        {
            ApplicationId = Id,
            PropertyId = PropertyId,
            ApplicantId = ApplicantId,
            ReviewedBy = reviewedBy,
            DecisionNotes = decisionNotes
        });
    }

    public void Reject(Guid reviewedBy, string? decisionNotes = null)
    {
        if (reviewedBy == Guid.Empty)
        {
            throw new ArgumentException("Reviewer ID cannot be empty", nameof(reviewedBy));
        }

        if (!SubmittedAt.HasValue)
        {
            throw new InvalidOperationException("Application must be submitted before it can be rejected");
        }

        if (Status == ApplicationStatus.Withdrawn)
        {
            throw new InvalidOperationException("Cannot reject a withdrawn application");
        }

        Status = ApplicationStatus.Rejected;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewedBy;
        DecisionNotes = decisionNotes;
        UpdateTimestamp();

        AddDomainEvent(new PropertyApplicationRejectedEvent
        {
            ApplicationId = Id,
            PropertyId = PropertyId,
            ApplicantId = ApplicantId,
            ReviewedBy = reviewedBy,
            DecisionNotes = decisionNotes
        });
    }

    public void Withdraw()
    {
        if (Status == ApplicationStatus.Approved)
        {
            throw new InvalidOperationException("Cannot withdraw an approved application");
        }

        if (Status == ApplicationStatus.Withdrawn)
        {
            throw new InvalidOperationException("Application has already been withdrawn");
        }

        Status = ApplicationStatus.Withdrawn;
        UpdateTimestamp();
    }
}

