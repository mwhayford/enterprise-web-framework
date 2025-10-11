using MediatR;

namespace Core.Application.Commands;

public record ProcessUserDataCommand : IRequest
{
    public Guid UserId { get; init; }
}
