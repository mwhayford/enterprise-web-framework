namespace Core.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, string? topic = null) where T : class;
}
