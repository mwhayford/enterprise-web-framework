// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Infrastructure.Persistence.Configurations;

public class LeaseConfiguration : IEntityTypeConfiguration<Lease>
{
    public void Configure(EntityTypeBuilder<Lease> builder)
    {
        builder.ToTable("Leases");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.PropertyId)
            .IsRequired();

        builder.Property(l => l.TenantId)
            .IsRequired();

        builder.Property(l => l.LandlordId)
            .IsRequired();

        builder.Property(l => l.StartDate)
            .IsRequired();

        builder.Property(l => l.EndDate)
            .IsRequired();

        builder.OwnsOne(l => l.MonthlyRent, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("MonthlyRent_Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("MonthlyRent_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(l => l.SecurityDeposit, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("SecurityDeposit_Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("SecurityDeposit_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(l => l.PaymentFrequency)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(l => l.PaymentDayOfMonth)
            .IsRequired();

        builder.Property(l => l.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(l => l.SpecialTerms)
            .HasMaxLength(5000);

        builder.Property(l => l.ActivatedAt);

        builder.Property(l => l.TerminatedAt);

        builder.Property(l => l.TerminationReason)
            .HasMaxLength(1000);

        builder.Property(l => l.PropertyApplicationId);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(l => l.PropertyId);
        builder.HasIndex(l => l.TenantId);
        builder.HasIndex(l => l.LandlordId);
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.StartDate);
        builder.HasIndex(l => l.EndDate);
        builder.HasIndex(l => l.PropertyApplicationId);
        builder.HasIndex(l => new { l.PropertyId, l.Status });
        builder.HasIndex(l => new { l.TenantId, l.Status });

        // Ignore domain events collection
        builder.Ignore(l => l.DomainEvents);
    }
}
