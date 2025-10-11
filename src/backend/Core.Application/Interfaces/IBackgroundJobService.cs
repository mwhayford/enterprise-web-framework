using System.Linq.Expressions;

namespace Core.Application.Interfaces;

public interface IBackgroundJobService
{
    string Enqueue<T>(Expression<Action<T>> methodCall);
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);
    string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
    string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt);
    string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);
    string Recurring<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression);
    string Recurring<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression);
    bool Delete(string jobId);
    bool Delete(string recurringJobId, string jobId);
    void Trigger(string recurringJobId);
}
