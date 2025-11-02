// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.Extensions.Logging;
using RentalManager.Application.Interfaces;

namespace RentalManager.Infrastructure.Services;

public class RecurringJobsService
{
    private readonly ILogger<RecurringJobsService> _logger;
    private readonly IBackgroundJobService _backgroundJobService;

    public RecurringJobsService(ILogger<RecurringJobsService> logger, IBackgroundJobService backgroundJobService)
    {
        _logger = logger;
        _backgroundJobService = backgroundJobService;
    }

    public void ConfigureRecurringJobs()
    {
        _logger.LogInformation("Configuring recurring background jobs");

        // Clean up expired sessions every hour
        _backgroundJobService.Recurring<DataProcessingService>(
            "cleanup-expired-sessions",
            service => service.CleanupExpiredSessionsAsync(),
            "0 * * * *"); // Every hour

        // Update search index every 6 hours
        _backgroundJobService.Recurring<DataProcessingService>(
            "update-search-index",
            service => service.UpdateSearchIndexAsync(),
            "0 */6 * * *"); // Every 6 hours

        // Process payment reconciliation daily at 2 AM
        _backgroundJobService.Recurring<DataProcessingService>(
            "payment-reconciliation",
            service => service.ProcessPaymentReconciliationAsync(),
            "0 2 * * *"); // Daily at 2 AM

        _logger.LogInformation("Recurring background jobs configured successfully");
    }
}
