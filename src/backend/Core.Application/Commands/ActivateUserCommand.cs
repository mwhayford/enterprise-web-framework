using MediatR;

namespace Core.Application.Commands;

public record ActivateUserCommand : IRequest<bool>
{
    public Guid UserId { get; init; }
}
