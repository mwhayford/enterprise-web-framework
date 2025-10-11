using Core.Domain.Entities;
using Core.Domain.ValueObjects;

namespace Core.Application.Interfaces;

public interface IPaymentService
{
    Task<Payment> ProcessPaymentAsync(
        Guid userId,
        Money amount,
        PaymentMethodType paymentMethodType,
        string? paymentMethodId = null,
        string? description = null);

    Task<Payment> ProcessSubscriptionPaymentAsync(
        Guid userId,
        string planId,
        Money amount,
        string? paymentMethodId = null);

    Task<bool> RefundPaymentAsync(Guid paymentId, Money? amount = null);
    Task<bool> CancelPaymentAsync(Guid paymentId);
    Task<Payment?> GetPaymentAsync(Guid paymentId);
    Task<IEnumerable<Payment>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20);
}
