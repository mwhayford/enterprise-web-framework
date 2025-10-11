using MediatR;

namespace Core.Application.Commands;

public record DeactivateUserCommand : IRequest<bool>
{
    public Guid UserId { get; init; }
}
