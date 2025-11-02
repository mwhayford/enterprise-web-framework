// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Infrastructure.Persistence.Configurations;

public class ApplicationSettingsConfiguration : IEntityTypeConfiguration<ApplicationSettings>
{
    public void Configure(EntityTypeBuilder<ApplicationSettings> builder)
    {
        builder.ToTable("ApplicationSettings");

        builder.HasKey(s => s.Id);

        builder.OwnsOne(s => s.DefaultApplicationFee, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("DefaultApplicationFee_Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("DefaultApplicationFee_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(s => s.ApplicationFeeEnabled)
            .IsRequired();

        builder.Property(s => s.RequirePaymentUpfront)
            .IsRequired();

        builder.Property(s => s.MaxApplicationsPerUser);

        builder.Property(s => s.ApplicationFormFields)
            .HasColumnType("jsonb");

        builder.Property(s => s.UpdatedBy);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        // Ignore domain events collection
        builder.Ignore(s => s.DomainEvents);
    }
}
