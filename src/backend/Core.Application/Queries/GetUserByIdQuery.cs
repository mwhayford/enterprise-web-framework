using MediatR;
using Core.Application.DTOs;

namespace Core.Application.Queries;

public record GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid UserId { get; init; }
}
