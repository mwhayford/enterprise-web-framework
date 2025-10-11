using Core.Domain.ValueObjects;

namespace Core.Application.DTOs;

public record PaymentMethodDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public PaymentMethodType Type { get; init; }
    public string? LastFourDigits { get; init; }
    public string? Brand { get; init; }
    public string? BankName { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public string DisplayName { get; init; } = string.Empty;
}
