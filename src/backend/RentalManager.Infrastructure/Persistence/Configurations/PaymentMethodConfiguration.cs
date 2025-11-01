// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Infrastructure.Persistence.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.UserId)
            .IsRequired();

        builder.Property(pm => pm.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pm => pm.StripePaymentMethodId)
            .HasMaxLength(100);

        builder.Property(pm => pm.LastFourDigits)
            .HasMaxLength(4);

        builder.Property(pm => pm.Brand)
            .HasMaxLength(50);

        builder.Property(pm => pm.BankName)
            .HasMaxLength(100);

        builder.Property(pm => pm.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pm => pm.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(pm => pm.CreatedAt)
            .IsRequired();

        builder.Property(pm => pm.UpdatedAt)
            .IsRequired();

        builder.HasIndex(pm => pm.UserId);
        builder.HasIndex(pm => pm.StripePaymentMethodId);
        builder.HasIndex(pm => pm.IsDefault);
        builder.HasIndex(pm => pm.IsActive);

        // User relationship is handled by ASP.NET Core Identity
    }
}
