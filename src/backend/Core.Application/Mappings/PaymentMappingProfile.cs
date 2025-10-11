using AutoMapper;
using Core.Application.DTOs;
using Core.Domain.Entities;

namespace Core.Application.Mappings;

public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Amount.Currency));
    }
}

public static class PaymentExtensions
{
    public static PaymentDto ToDto(this Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            UserId = payment.UserId,
            Amount = payment.Amount.Amount,
            Currency = payment.Amount.Currency,
            Status = payment.Status,
            PaymentMethodType = payment.PaymentMethodType,
            Description = payment.Description,
            FailureReason = payment.FailureReason,
            CreatedAt = payment.CreatedAt,
            ProcessedAt = payment.ProcessedAt
        };
    }
}
