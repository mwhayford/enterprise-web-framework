// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Application.Interfaces;

namespace Core.Infrastructure.Services;

public class EventPublisher : IEventPublisher
{
    private readonly IEventBus _eventBus;

    public EventPublisher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task PublishAsync<T>(T @event, string? topic = null) where T : class
    {
        await _eventBus.PublishAsync(@event, topic);
    }
}
