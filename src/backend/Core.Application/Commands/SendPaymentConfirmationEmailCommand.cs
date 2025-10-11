using MediatR;

namespace Core.Application.Commands;

public record SendPaymentConfirmationEmailCommand : IRequest
{
    public string Email { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}
