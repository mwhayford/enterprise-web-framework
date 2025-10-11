using MediatR;
using Core.Application.DTOs;

namespace Core.Application.Queries;

public record GetUsersQuery : IRequest<PaginatedResult<UserDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
}
