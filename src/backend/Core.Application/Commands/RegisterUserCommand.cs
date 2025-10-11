using MediatR;
using Core.Application.DTOs;

namespace Core.Application.Commands;

public record RegisterUserCommand : IRequest<UserDto>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? GoogleId { get; init; }
    public string? ProfilePictureUrl { get; init; }
}
