// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using RentalManager.Application.Interfaces;

namespace RentalManager.Infrastructure.Services;

/// <summary>
/// No-op implementation of IBackgroundJobService used when Hangfire is not available.
/// All methods log a warning and return empty/null values.
/// </summary>
public class NullBackgroundJobService : IBackgroundJobService
{
    private readonly ILogger<NullBackgroundJobService> _logger;

    public NullBackgroundJobService(ILogger<NullBackgroundJobService> logger)
    {
        _logger = logger;
    }

    public string Enqueue<T>(Expression<Action<T>> methodCall)
    {
        _logger.LogWarning("Background job service is not available. Hangfire is not configured. Job '{JobName}' will not be executed.", methodCall.ToString());
        return string.Empty;
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        _logger.LogWarning("Background job service is not available. Hangfire is not configured. Job '{JobName}' will not be executed.", methodCall.ToString());
        return string.Empty;
    }

    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
    {
        _logger.LogWarning("Background job service is not available. Hangfire is not configured. Scheduled job '{JobName}' will not be executed.", methodCall.ToString());
        return string.Empty;
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        _logger.LogWarning("Background job service is not available. Hangfire is not configured. Scheduled job '{JobName}' will not be executed.", methodCall.ToString());
        return string.Empty;
    }

    public string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt)
    {
        _logger.LogWarning("Background job service is not available. Hangfire is not configured. Scheduled job '{JobName}' will not be executed.", methodCall.ToString());
        return string.Empty;
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
    {
        _logger.LogWarning("Background job service is not available. Hangfire is not configured. Scheduled job '{JobName}' will not be executed.", methodCall.ToString());
        return string.Empty;
    }

    public string Recurring<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression)
    {
        _logger.LogWarning("Background job service is not available. Hangfire is not configured. Recurring job '{RecurringJobId}' will not be executed.", recurringJobId);
        return recurringJobId;
    }

    public string Recurring<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression)
    {
        _logger.LogWarning("Background job service is not available. Hangfire is not configured. Recurring job '{RecurringJobId}' will not be executed.", recurringJobId);
        return recurringJobId;
    }

    public bool Delete(string jobId)
    {
        _logger.LogWarning("Background job service is not available. Hangfire is not configured. Cannot delete job '{JobId}'.", jobId);
        return false;
    }

    public bool Delete(string recurringJobId, string jobId)
    {
        _logger.LogWarning("Background job service is not available. Hangfire is not configured. Cannot delete recurring job '{RecurringJobId}' job '{JobId}'.", recurringJobId, jobId);
        return false;
    }

    public void Trigger(string recurringJobId)
    {
        _logger.LogWarning("Background job service is not available. Hangfire is not configured. Cannot trigger recurring job '{RecurringJobId}'.", recurringJobId);
    }
}

