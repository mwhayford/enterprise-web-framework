using MediatR;
using Core.Application.Commands;
using Core.Application.Interfaces;
using Core.Infrastructure.Services;

namespace Core.Infrastructure.Handlers;

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
