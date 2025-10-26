// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Infrastructure.Persistence.Configurations;

public class PropertyApplicationConfiguration : IEntityTypeConfiguration<PropertyApplication>
{
    public void Configure(EntityTypeBuilder<PropertyApplication> builder)
    {
        builder.ToTable("PropertyApplications");

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.PropertyId)
            .IsRequired();

        builder.Property(pa => pa.ApplicantId)
            .IsRequired();

        builder.Property(pa => pa.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(pa => pa.ApplicationData)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.OwnsOne(pa => pa.ApplicationFee, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ApplicationFee_Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("ApplicationFee_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(pa => pa.ApplicationFeePaymentId);

        builder.Property(pa => pa.SubmittedAt);

        builder.Property(pa => pa.ReviewedAt);

        builder.Property(pa => pa.ReviewedBy);

        builder.Property(pa => pa.DecisionNotes)
            .HasMaxLength(1000);

        builder.Property(pa => pa.CreatedAt)
            .IsRequired();

        builder.Property(pa => pa.UpdatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(pa => pa.PropertyId);
        builder.HasIndex(pa => pa.ApplicantId);
        builder.HasIndex(pa => pa.Status);
        builder.HasIndex(pa => pa.SubmittedAt);
        builder.HasIndex(pa => pa.CreatedAt);

        // Ignore domain events collection
        builder.Ignore(pa => pa.DomainEvents);
    }
}


