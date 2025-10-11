using MediatR;
using Core.Application.DTOs;

namespace Core.Application.Commands;

public record UpdateUserProfileCommand : IRequest<UserDto>
{
    public Guid UserId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? ProfilePictureUrl { get; init; }
}
