// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentalManager.Application.Interfaces;
using RentalManager.Domain.Entities;
using RentalManager.Domain.Interfaces;
using RentalManager.Infrastructure.Identity;
using RentalManager.Infrastructure.Persistence.Configurations;

namespace RentalManager.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly ILogger<ApplicationDbContext>? _logger;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        // Access IServiceProvider from DbContextOptions extension
        // This is automatically set when AddDbContext uses the factory pattern with serviceProvider parameter
        var extension = options.FindExtension<Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension>();
        _serviceProvider = extension?.ApplicationServiceProvider;

        // Get logger from service provider if available
        _logger = _serviceProvider?.GetService<ILogger<ApplicationDbContext>>();
    }

    // Users are managed by ASP.NET Core Identity via base class
    public DbSet<Payment> Payments { get; set; }

    public DbSet<Subscription> Subscriptions { get; set; }

    public DbSet<PaymentMethod> PaymentMethods { get; set; }

    public DbSet<Property> Properties { get; set; }

    public DbSet<PropertyApplication> PropertyApplications { get; set; }

    public DbSet<Lease> Leases { get; set; }

    public DbSet<WorkOrder> WorkOrders { get; set; }

    public DbSet<ApplicationSettings> ApplicationSettings { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events from all entities before saving
        var domainEvents = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // Save changes to database first
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events to Kafka after successful save
        if (domainEvents.Any())
        {
            await DispatchDomainEventsAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    public override int SaveChanges()
    {
        // Collect domain events from all entities before saving
        var domainEvents = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // Save changes to database first
        var result = base.SaveChanges();

        // Dispatch domain events to Kafka after successful save
        // Note: Using async fire-and-forget for sync SaveChanges to avoid blocking
        if (domainEvents.Any() && _serviceProvider != null)
        {
            // Fire-and-forget async dispatch (events are logged if publishing fails)
            _ = Task.Run(async () =>
            {
                try
                {
                    await DispatchDomainEventsAsync(domainEvents, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error dispatching domain events in SaveChanges");
                }
            });
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration is handled by ASP.NET Core Identity
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMethodConfiguration());
        modelBuilder.ApplyConfiguration(new PropertyConfiguration());
        modelBuilder.ApplyConfiguration(new PropertyApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new LeaseConfiguration());
        modelBuilder.ApplyConfiguration(new WorkOrderConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationSettingsConfiguration());
    }

    private async Task DispatchDomainEventsAsync(List<IDomainEvent> domainEvents, CancellationToken _)
    {
        if (_serviceProvider == null)
        {
            _logger?.LogWarning("IServiceProvider not available. Domain events will not be published.");
            return;
        }

        // Lazy resolve IEventPublisher to avoid circular dependency issues
        var eventPublisher = _serviceProvider.GetService<IEventPublisher>();
        if (eventPublisher == null)
        {
            _logger?.LogWarning("IEventPublisher not available. Domain events will not be published.");
            return;
        }

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                // Use reflection to call PublishAsync with the concrete event type
                var eventType = domainEvent.GetType();
                var publishMethod = typeof(IEventPublisher).GetMethod(nameof(IEventPublisher.PublishAsync))
                    ?.MakeGenericMethod(eventType);

                if (publishMethod != null)
                {
                    var parameters = new object?[] { domainEvent, null };
                    var publishTask = (Task)publishMethod.Invoke(eventPublisher, parameters)!;
                    await publishTask;
                    _logger?.LogDebug("Published domain event {EventType} with ID {EventId}", eventType.Name, domainEvent.Id);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the save operation
                // The database transaction has already succeeded
                _logger?.LogError(ex, "Failed to publish domain event {EventType} with ID {EventId}. Event will be lost.", domainEvent.GetType().Name, domainEvent.Id);
            }
        }

        // Clear domain events from all entities after publishing
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.ClearDomainEvents();
        }
    }
}
