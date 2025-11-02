// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using RentalManager.Application.Commands;
using RentalManager.Application.Interfaces;
using RentalManager.Infrastructure.Services;

namespace RentalManager.Infrastructure.Handlers;

public class ProcessUserDataCommandHandler : IRequestHandler<ProcessUserDataCommand>
{
    private readonly IBackgroundJobService _backgroundJobService;

    public ProcessUserDataCommandHandler(IBackgroundJobService backgroundJobService)
    {
        _backgroundJobService = backgroundJobService;
    }

    public async Task Handle(ProcessUserDataCommand request, CancellationToken cancellationToken)
    {
        // Enqueue the data processing as a background job
        _backgroundJobService.Enqueue<DataProcessingService>(service =>
            service.ProcessUserDataAsync(request.UserId));

        await Task.CompletedTask;
    }
}
