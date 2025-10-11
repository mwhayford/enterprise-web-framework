namespace Core.Domain.Interfaces;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
