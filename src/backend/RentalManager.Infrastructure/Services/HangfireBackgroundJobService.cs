// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Linq.Expressions;
using Hangfire;
using RentalManager.Application.Interfaces;

namespace RentalManager.Infrastructure.Services;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireBackgroundJobService(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }

    public string Enqueue<T>(Expression<Action<T>> methodCall)
    {
        return _backgroundJobClient.Enqueue(methodCall);
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        return _backgroundJobClient.Enqueue(methodCall);
    }

    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
    {
        return _backgroundJobClient.Schedule(methodCall, delay);
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        return _backgroundJobClient.Schedule(methodCall, delay);
    }

    public string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt)
    {
        return _backgroundJobClient.Schedule(methodCall, enqueueAt.DateTime);
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
    {
        return _backgroundJobClient.Schedule(methodCall, enqueueAt.DateTime);
    }

    public string Recurring<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression)
    {
        _recurringJobManager.AddOrUpdate(recurringJobId, methodCall, cronExpression);
        return recurringJobId;
    }

    public string Recurring<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression)
    {
        _recurringJobManager.AddOrUpdate(recurringJobId, methodCall, cronExpression);
        return recurringJobId;
    }

    public bool Delete(string jobId)
    {
        return _backgroundJobClient.Delete(jobId);
    }

    public bool Delete(string recurringJobId, string jobId)
    {
        _recurringJobManager.RemoveIfExists(recurringJobId);
        return _backgroundJobClient.Delete(jobId);
    }

    [Obsolete]
    public void Trigger(string recurringJobId)
    {
        _recurringJobManager.Trigger(recurringJobId);
    }
}
