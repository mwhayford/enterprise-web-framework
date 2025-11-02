// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using RentalManager.Application.Interfaces;

namespace RentalManager.Infrastructure.Services;

/// <summary>
/// No-op implementation of IEventBus used when Kafka is not available (e.g., in tests).
/// All methods log and return successfully without actually publishing events.
/// </summary>
public class NullEventBus : IEventBus
{
    private readonly ILogger<NullEventBus> _logger;

    public NullEventBus(ILogger<NullEventBus> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(T @event, string? topic = null)
        where T : class
    {
        var eventType = typeof(T).Name;
        _logger.LogDebug("Event bus is not available. Event '{EventType}' would be published to topic '{Topic}' but will be ignored.", eventType, topic ?? eventType.ToLowerInvariant());
        return Task.CompletedTask;
    }

    public Task SubscribeAsync<T>(string topic, Func<T, Task> handler)
        where T : class
    {
        _logger.LogDebug("Event bus is not available. Subscription to topic '{Topic}' for event '{EventType}' will be ignored.", topic, typeof(T).Name);
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NullEventBus started (no-op).");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NullEventBus stopped (no-op).");
        return Task.CompletedTask;
    }
}

