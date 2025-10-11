// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly ApplicationDbContext _context;

    public PaymentMethodService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentMethod> CreatePaymentMethodAsync(
        Guid userId,
        PaymentMethodType type,
        string stripePaymentMethodId,
        string? lastFourDigits = null,
        string? brand = null,
        string? bankName = null,
        bool isDefault = false)
    {
        // If this is set as default, unset other default payment methods for this user
        if (isDefault)
        {
            var existingDefaults = await _context.PaymentMethods
                .Where(pm => pm.UserId == userId && pm.IsDefault)
                .ToListAsync();

            foreach (var existingDefault in existingDefaults)
            {
                existingDefault.SetDefault(false);
            }
        }

        var paymentMethod = new PaymentMethod(
            userId,
            type,
            stripePaymentMethodId,
            lastFourDigits,
            brand,
            bankName,
            isDefault);

        _context.PaymentMethods.Add(paymentMethod);
        await _context.SaveChangesAsync();

        return paymentMethod;
    }

    public async Task<PaymentMethod?> GetPaymentMethodAsync(Guid paymentMethodId)
    {
        return await _context.PaymentMethods.FindAsync(paymentMethodId);
    }

    public async Task<IEnumerable<PaymentMethod>> GetUserPaymentMethodsAsync(Guid userId)
    {
        return await _context.PaymentMethods
            .Where(pm => pm.UserId == userId && pm.IsActive)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> DeletePaymentMethodAsync(Guid paymentMethodId)
    {
        var paymentMethod = await _context.PaymentMethods.FindAsync(paymentMethodId);
        if (paymentMethod == null)
        {
            return false;
        }

        paymentMethod.Deactivate();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetDefaultPaymentMethodAsync(Guid paymentMethodId)
    {
        var paymentMethod = await _context.PaymentMethods.FindAsync(paymentMethodId);
        if (paymentMethod == null)
        {
            return false;
        }

        // Unset other default payment methods for this user
        var existingDefaults = await _context.PaymentMethods
            .Where(pm => pm.UserId == paymentMethod.UserId && pm.IsDefault && pm.Id != paymentMethodId)
            .ToListAsync();

        foreach (var existingDefault in existingDefaults)
        {
            existingDefault.SetDefault(false);
        }

        // Set this payment method as default
        paymentMethod.SetDefault(true);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePaymentMethodAsync(Guid paymentMethodId, string? lastFourDigits = null, string? brand = null, string? bankName = null)
    {
        var paymentMethod = await _context.PaymentMethods.FindAsync(paymentMethodId);
        if (paymentMethod == null)
        {
            return false;
        }

        paymentMethod.UpdateDetails(lastFourDigits, brand, bankName);
        await _context.SaveChangesAsync();
        return true;
    }
}
