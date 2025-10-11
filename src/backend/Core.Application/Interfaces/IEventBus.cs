// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Threading.Tasks;

namespace Core.Application.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, string? topic = null) where T : class;
    Task SubscribeAsync<T>(string topic, Func<T, Task> handler) where T : class;
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
