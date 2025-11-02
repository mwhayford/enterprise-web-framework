// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Infrastructure.Persistence.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.PropertyId)
            .IsRequired();

        builder.Property(w => w.LeaseId)
            .IsRequired();

        builder.Property(w => w.RequestedBy)
            .IsRequired();

        builder.Property(w => w.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.Description)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(w => w.Category)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(w => w.Priority)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(w => w.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(w => w.AssignedTo);

        builder.Property(w => w.ApprovedBy);

        builder.Property(w => w.ApprovedAt);

        builder.Property(w => w.AssignedAt);

        builder.Property(w => w.StartedAt);

        builder.Property(w => w.CompletedAt);

        builder.Property(w => w.EstimatedCost)
            .HasPrecision(18, 2);

        builder.Property(w => w.ActualCost)
            .HasPrecision(18, 2);

        builder.Property(w => w.Notes)
            .HasMaxLength(5000);

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .IsRequired();

        // Store images as JSON
        builder.Property<List<string>>("_images")
            .HasColumnName("Images")
            .HasColumnType("jsonb");

        // Indexes
        builder.HasIndex(w => w.PropertyId);
        builder.HasIndex(w => w.LeaseId);
        builder.HasIndex(w => w.RequestedBy);
        builder.HasIndex(w => w.AssignedTo);
        builder.HasIndex(w => w.Status);
        builder.HasIndex(w => w.CreatedAt);
        builder.HasIndex(w => new { w.PropertyId, w.Status });
        builder.HasIndex(w => new { w.RequestedBy, w.Status });
        builder.HasIndex(w => new { w.AssignedTo, w.Status });

        // Ignore domain events collection
        builder.Ignore(w => w.DomainEvents);
    }
}
