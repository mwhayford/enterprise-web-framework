// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RentalManager.Application.Handlers;

public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserCreatedEvent for user {UserId} with email {Email}",
            notification.UserId,
            notification.Email);

        // Here you can add business logic like:
        // - Send welcome email
        // - Create user profile
        // - Initialize user settings
        // - Update analytics
        // - Index user in search
        await Task.CompletedTask;
    }
}
