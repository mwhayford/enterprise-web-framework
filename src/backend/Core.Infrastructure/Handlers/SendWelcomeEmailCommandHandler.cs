// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Core.Application.Commands;
using Core.Application.Interfaces;
using Core.Infrastructure.Services;

namespace Core.Infrastructure.Handlers;

public class SendWelcomeEmailCommandHandler : IRequestHandler<SendWelcomeEmailCommand>
{
    private readonly IBackgroundJobService _backgroundJobService;

    public SendWelcomeEmailCommandHandler(IBackgroundJobService backgroundJobService)
    {
        _backgroundJobService = backgroundJobService;
    }

    public async Task Handle(SendWelcomeEmailCommand request, CancellationToken cancellationToken)
    {
        // Enqueue the email sending as a background job
        _backgroundJobService.Enqueue<EmailService>(service => 
            service.SendWelcomeEmailAsync(request.Email, request.FirstName));
        
        await Task.CompletedTask;
    }
}
