// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace Core.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, string? topic = null) where T : class;
}
