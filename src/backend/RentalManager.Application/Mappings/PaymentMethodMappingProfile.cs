// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using AutoMapper;
using RentalManager.Application.DTOs;
using RentalManager.Domain.Entities;

namespace RentalManager.Application.Mappings;

public static class PaymentMethodExtensions
{
    public static PaymentMethodDto ToDto(this PaymentMethod paymentMethod)
    {
        return new PaymentMethodDto
        {
            Id = paymentMethod.Id,
            UserId = paymentMethod.UserId,
            Type = paymentMethod.Type,
            StripePaymentMethodId = paymentMethod.StripePaymentMethodId,
            LastFourDigits = paymentMethod.LastFourDigits,
            Brand = paymentMethod.Brand,
            BankName = paymentMethod.BankName,
            IsDefault = paymentMethod.IsDefault,
            IsActive = paymentMethod.IsActive,
            CreatedAt = paymentMethod.CreatedAt,
            UpdatedAt = paymentMethod.UpdatedAt,
            DisplayName = paymentMethod.DisplayName
        };
    }
}

public class PaymentMethodMappingProfile : Profile
{
    public PaymentMethodMappingProfile()
    {
        CreateMap<PaymentMethod, PaymentMethodDto>()
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName));
    }
}
