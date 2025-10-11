// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace Core.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, string? topic = null)
        where T : class;
}
