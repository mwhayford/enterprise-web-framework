// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalManager.Domain.Entities;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.OwnerId)
            .IsRequired();

        builder.OwnsOne(p => p.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Address_Street")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(a => a.Unit)
                .HasColumnName("Address_Unit")
                .HasMaxLength(50);

            address.Property(a => a.City)
                .HasColumnName("Address_City")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.State)
                .HasColumnName("Address_State")
                .HasMaxLength(50)
                .IsRequired();

            address.Property(a => a.ZipCode)
                .HasColumnName("Address_ZipCode")
                .HasMaxLength(20)
                .IsRequired();

            address.Property(a => a.Country)
                .HasColumnName("Address_Country")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(p => p.PropertyType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Bedrooms)
            .IsRequired();

        builder.Property(p => p.Bathrooms)
            .IsRequired();

        builder.Property(p => p.SquareFeet)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.OwnsOne(p => p.MonthlyRent, money =>
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

        builder.OwnsOne(p => p.SecurityDeposit, money =>
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

        builder.Property(p => p.AvailableDate)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.OwnsOne(p => p.ApplicationFee, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ApplicationFee_Amount")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("ApplicationFee_Currency")
                .HasMaxLength(3);
        });

        // Store amenities as JSON
        builder.Property<List<string>>("_amenities")
            .HasColumnName("Amenities")
            .HasColumnType("jsonb");

        // Store images as JSON
        builder.Property<List<string>>("_images")
            .HasColumnName("Images")
            .HasColumnType("jsonb");

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.AvailableDate);
        builder.HasIndex(p => p.CreatedAt);

        // Ignore domain events collection
        builder.Ignore(p => p.DomainEvents);
    }
}
