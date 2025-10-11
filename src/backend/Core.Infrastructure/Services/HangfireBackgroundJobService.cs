// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Linq.Expressions;
using Hangfire;
using Core.Application.Interfaces;

namespace Core.Infrastructure.Services;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    public string Enqueue<T>(Expression<Action<T>> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }

    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
    {
        return BackgroundJob.Schedule(methodCall, delay);
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        return BackgroundJob.Schedule(methodCall, delay);
    }

    public string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt)
    {
        return BackgroundJob.Schedule(methodCall, enqueueAt);
    }

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt)
    {
        return BackgroundJob.Schedule(methodCall, enqueueAt);
    }

    public string Recurring<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression)
    {
        RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression);
        return recurringJobId;
    }

    public string Recurring<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression)
    {
        RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression);
        return recurringJobId;
    }

    public bool Delete(string jobId)
    {
        return BackgroundJob.Delete(jobId);
    }

    public bool Delete(string recurringJobId, string jobId)
    {
        RecurringJob.RemoveIfExists(recurringJobId);
        return BackgroundJob.Delete(jobId);
    }

    public void Trigger(string recurringJobId)
    {
        RecurringJob.Trigger(recurringJobId);
    }
}
