// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Application.Interfaces;

namespace RentalManager.Infrastructure.Services;

public class EventPublisher : IEventPublisher
{
    private readonly IEventBus _eventBus;

    public EventPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task PublishAsync<T>(T @event, string? topic = null)
        where T : class
    {
        await _eventBus.PublishAsync(@event, topic);
    }
}
