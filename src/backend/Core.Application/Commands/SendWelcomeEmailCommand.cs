using MediatR;

namespace Core.Application.Commands;

public record SendWelcomeEmailCommand : IRequest
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
}
