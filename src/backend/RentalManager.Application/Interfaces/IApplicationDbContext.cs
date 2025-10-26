// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.EntityFrameworkCore;
using RentalManager.Domain.Entities;

namespace RentalManager.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Payment> Payments { get; set; }

    DbSet<Subscription> Subscriptions { get; set; }

    DbSet<PaymentMethod> PaymentMethods { get; set; }

    DbSet<Property> Properties { get; set; }

    DbSet<PropertyApplication> PropertyApplications { get; set; }

    DbSet<ApplicationSettings> ApplicationSettings { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

