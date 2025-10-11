using MediatR;
using Core.Application.DTOs;

namespace Core.Application.Queries;

public record GetPaymentHistoryQuery : IRequest<IEnumerable<PaymentDto>>
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
