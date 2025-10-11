using MediatR;
using Core.Application.DTOs;

namespace Core.Application.Queries;

public record GetUserByEmailQuery : IRequest<UserDto?>
{
    public string Email { get; init; } = string.Empty;
}
