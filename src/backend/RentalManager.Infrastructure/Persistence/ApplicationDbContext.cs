// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.Interfaces;
using RentalManager.Domain.Entities;
using RentalManager.Infrastructure.Identity;
using RentalManager.Infrastructure.Persistence.Configurations;

namespace RentalManager.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Users are managed by ASP.NET Core Identity via base class
    public DbSet<Payment> Payments { get; set; }

    public DbSet<Subscription> Subscriptions { get; set; }

    public DbSet<PaymentMethod> PaymentMethods { get; set; }

    public DbSet<Property> Properties { get; set; }

    public DbSet<PropertyApplication> PropertyApplications { get; set; }

    public DbSet<ApplicationSettings> ApplicationSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration is handled by ASP.NET Core Identity
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMethodConfiguration());
        modelBuilder.ApplyConfiguration(new PropertyConfiguration());
        modelBuilder.ApplyConfiguration(new PropertyApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationSettingsConfiguration());
    }
}
