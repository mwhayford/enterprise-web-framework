// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using AutoMapper;
using RentalManager.Application.DTOs;
using RentalManager.Domain.Entities;

namespace RentalManager.Application.Mappings;

public static class SubscriptionExtensions
{
    public static SubscriptionDto ToDto(this Subscription subscription)
    {
        return new SubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            PlanId = subscription.PlanId,
            Amount = subscription.Amount.Amount,
            Currency = subscription.Amount.Currency,
            Status = subscription.Status,
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            TrialStart = subscription.TrialStart,
            TrialEnd = subscription.TrialEnd,
            CanceledAt = subscription.CanceledAt,
            CreatedAt = subscription.CreatedAt
        };
    }
}

public class SubscriptionMappingProfile : Profile
{
    public SubscriptionMappingProfile()
    {
        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Amount.Currency));
    }
}
