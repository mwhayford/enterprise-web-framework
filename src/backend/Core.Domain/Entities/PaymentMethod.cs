// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

public class PaymentMethod : BaseEntity
{
    public Guid UserId { get; private set; }
    public PaymentMethodType Type { get; private set; }
    public string? StripePaymentMethodId { get; private set; }
    public string? LastFourDigits { get; private set; }
    public string? Brand { get; private set; }
    public string? BankName { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    private PaymentMethod() { } // For EF Core

    public PaymentMethod(
        Guid userId,
        PaymentMethodType type,
        string? stripePaymentMethodId = null,
        string? lastFourDigits = null,
        string? brand = null,
        string? bankName = null,
        bool isDefault = false)
        : base()
    {
        UserId = userId;
        Type = type;
        StripePaymentMethodId = stripePaymentMethodId;
        LastFourDigits = lastFourDigits;
        Brand = brand;
        BankName = bankName;
        IsDefault = isDefault;
        IsActive = true;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdateTimestamp();
    }

    public void RemoveDefault()
    {
        IsDefault = false;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        IsDefault = false;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
        UpdateTimestamp();
    }

    public void UpdateDetails(string? lastFourDigits, string? brand, string? bankName)
    {
        if (!string.IsNullOrEmpty(lastFourDigits))
            LastFourDigits = lastFourDigits;
        if (!string.IsNullOrEmpty(brand))
            Brand = brand;
        if (!string.IsNullOrEmpty(bankName))
            BankName = bankName;
        UpdateTimestamp();
    }

    public string DisplayName
    {
        get
        {
            return Type switch
            {
                PaymentMethodType.Card => $"{Brand} •••• {LastFourDigits}",
                PaymentMethodType.Ach => $"{BankName} •••• {LastFourDigits}",
                _ => $"{Type} •••• {LastFourDigits}"
            };
        }
    }
}
