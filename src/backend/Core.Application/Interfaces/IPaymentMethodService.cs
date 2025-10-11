// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Domain.Entities;
using Core.Domain.ValueObjects;

namespace Core.Application.Interfaces;

public interface IPaymentMethodService
{
    Task<PaymentMethod> CreatePaymentMethodAsync(
        Guid userId,
        PaymentMethodType type,
        string stripePaymentMethodId,
        string? lastFourDigits = null,
        string? brand = null,
        string? bankName = null,
        bool isDefault = false);

    Task<PaymentMethod?> GetPaymentMethodAsync(Guid paymentMethodId);
    Task<IEnumerable<PaymentMethod>> GetUserPaymentMethodsAsync(Guid userId);
    Task<bool> DeletePaymentMethodAsync(Guid paymentMethodId);
    Task<bool> SetDefaultPaymentMethodAsync(Guid paymentMethodId);
    Task<bool> UpdatePaymentMethodAsync(Guid paymentMethodId, string? lastFourDigits = null, string? brand = null, string? bankName = null);
}
