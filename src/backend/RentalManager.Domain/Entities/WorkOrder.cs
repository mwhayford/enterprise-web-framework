// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.Events;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Domain.Entities;

public class WorkOrder : BaseEntity
{
    private readonly List<string> _images = new();

    public WorkOrder(
        Guid propertyId,
        Guid leaseId,
        Guid requestedBy,
        string title,
        string description,
        WorkOrderCategory category,
        WorkOrderPriority priority)
        : base()
    {
        if (propertyId == Guid.Empty)
        {
            throw new ArgumentException("Property ID cannot be empty", nameof(propertyId));
        }

        if (leaseId == Guid.Empty)
        {
            throw new ArgumentException("Lease ID cannot be empty", nameof(leaseId));
        }

        if (requestedBy == Guid.Empty)
        {
            throw new ArgumentException("RequestedBy ID cannot be empty", nameof(requestedBy));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description cannot be empty", nameof(description));
        }

        PropertyId = propertyId;
        LeaseId = leaseId;
        RequestedBy = requestedBy;
        Title = title;
        Description = description;
        Category = category;
        Priority = priority;
        Status = WorkOrderStatus.Requested;

        AddDomainEvent(new WorkOrderCreatedEvent
        {
            WorkOrderId = Id,
            PropertyId = propertyId,
            LeaseId = leaseId,
            RequestedBy = requestedBy,
            Title = title,
            Category = category,
            Priority = priority
        });
    }

    private WorkOrder()
    {
        Title = default!;
        Description = default!;
    } // For EF Core

    public Guid PropertyId { get; private set; }

    public Guid LeaseId { get; private set; }

    public Guid RequestedBy { get; private set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public WorkOrderCategory Category { get; private set; }

    public WorkOrderPriority Priority { get; private set; }

    public WorkOrderStatus Status { get; private set; }

    public Guid? AssignedTo { get; private set; }

    public Guid? ApprovedBy { get; private set; }

    public DateTime? ApprovedAt { get; private set; }

    public DateTime? AssignedAt { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public decimal? EstimatedCost { get; private set; }

    public decimal? ActualCost { get; private set; }

    public string? Notes { get; private set; }

    public IReadOnlyCollection<string> Images => _images.AsReadOnly();

    public bool IsRequested => Status == WorkOrderStatus.Requested;

    public bool IsApproved => Status == WorkOrderStatus.Approved;

    public bool IsAssigned => Status == WorkOrderStatus.Assigned && AssignedTo.HasValue;

    public bool IsInProgress => Status == WorkOrderStatus.InProgress;

    public bool IsCompleted => Status == WorkOrderStatus.Completed;

    public bool IsCancelled => Status == WorkOrderStatus.Cancelled;

    public bool CanBeApproved => Status == WorkOrderStatus.Requested;

    public bool CanBeRejected => Status == WorkOrderStatus.Requested;

    public bool CanBeAssigned => Status == WorkOrderStatus.Approved;

    public bool CanBeStarted => Status == WorkOrderStatus.Assigned && AssignedTo.HasValue;

    public bool CanBeCompleted => Status == WorkOrderStatus.InProgress;

    public bool CanBeCancelled => Status != WorkOrderStatus.Completed && Status != WorkOrderStatus.Cancelled;

    public void Approve(Guid approvedBy)
    {
        if (approvedBy == Guid.Empty)
        {
            throw new ArgumentException("ApprovedBy ID cannot be empty", nameof(approvedBy));
        }

        if (!CanBeApproved)
        {
            throw new InvalidOperationException("Work order cannot be approved in its current state");
        }

        Status = WorkOrderStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        UpdateTimestamp();

        AddDomainEvent(new WorkOrderApprovedEvent
        {
            WorkOrderId = Id,
            PropertyId = PropertyId,
            ApprovedBy = approvedBy,
            ApprovedAt = ApprovedAt.Value
        });
    }

    public void Reject(Guid rejectedBy, string? reason = null)
    {
        if (rejectedBy == Guid.Empty)
        {
            throw new ArgumentException("RejectedBy ID cannot be empty", nameof(rejectedBy));
        }

        if (!CanBeRejected)
        {
            throw new InvalidOperationException("Work order cannot be rejected in its current state");
        }

        Status = WorkOrderStatus.Cancelled;
        ApprovedBy = rejectedBy;
        ApprovedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = $"Rejected: {reason}";
        }

        UpdateTimestamp();

        AddDomainEvent(new WorkOrderRejectedEvent
        {
            WorkOrderId = Id,
            PropertyId = PropertyId,
            RejectedBy = rejectedBy,
            Reason = reason
        });
    }

    public void Assign(Guid assignedTo)
    {
        if (assignedTo == Guid.Empty)
        {
            throw new ArgumentException("AssignedTo ID cannot be empty", nameof(assignedTo));
        }

        if (!CanBeAssigned)
        {
            throw new InvalidOperationException("Work order cannot be assigned in its current state");
        }

        Status = WorkOrderStatus.Assigned;
        AssignedTo = assignedTo;
        AssignedAt = DateTime.UtcNow;
        UpdateTimestamp();

        AddDomainEvent(new WorkOrderAssignedEvent
        {
            WorkOrderId = Id,
            PropertyId = PropertyId,
            AssignedTo = assignedTo,
            AssignedAt = AssignedAt.Value
        });
    }

    public void Start()
    {
        if (!CanBeStarted)
        {
            throw new InvalidOperationException("Work order cannot be started in its current state");
        }

        Status = WorkOrderStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        UpdateTimestamp();

        AddDomainEvent(new WorkOrderStartedEvent
        {
            WorkOrderId = Id,
            PropertyId = PropertyId,
            AssignedTo = AssignedTo!.Value,
            StartedAt = StartedAt.Value
        });
    }

    public void Complete(decimal? actualCost = null, string? notes = null)
    {
        if (!CanBeCompleted)
        {
            throw new InvalidOperationException("Work order cannot be completed in its current state");
        }

        Status = WorkOrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        if (actualCost.HasValue)
        {
            ActualCost = actualCost;
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = string.IsNullOrWhiteSpace(Notes) ? notes : $"{Notes}\nCompleted: {notes}";
        }

        UpdateTimestamp();

        AddDomainEvent(new WorkOrderCompletedEvent
        {
            WorkOrderId = Id,
            PropertyId = PropertyId,
            AssignedTo = AssignedTo!.Value,
            CompletedAt = CompletedAt.Value,
            ActualCost = actualCost
        });
    }

    public void Cancel(string? reason = null)
    {
        if (!CanBeCancelled)
        {
            throw new InvalidOperationException("Work order cannot be cancelled in its current state");
        }

        Status = WorkOrderStatus.Cancelled;
        UpdateTimestamp();

        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = string.IsNullOrWhiteSpace(Notes) ? $"Cancelled: {reason}" : $"{Notes}\nCancelled: {reason}";
        }

        AddDomainEvent(new WorkOrderCancelledEvent
        {
            WorkOrderId = Id,
            PropertyId = PropertyId,
            Reason = reason
        });
    }

    public void UpdateEstimatedCost(decimal? estimatedCost)
    {
        if (estimatedCost.HasValue && estimatedCost.Value < 0)
        {
            throw new ArgumentException("Estimated cost cannot be negative", nameof(estimatedCost));
        }

        EstimatedCost = estimatedCost;
        UpdateTimestamp();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdateTimestamp();
    }

    public void AddImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new ArgumentException("Image URL cannot be empty", nameof(imageUrl));
        }

        if (!_images.Contains(imageUrl))
        {
            _images.Add(imageUrl);
            UpdateTimestamp();
        }
    }

    public void RemoveImage(string imageUrl)
    {
        _images.Remove(imageUrl);
        UpdateTimestamp();
    }
}

