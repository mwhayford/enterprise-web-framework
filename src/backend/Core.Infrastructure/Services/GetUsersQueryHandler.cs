using MediatR;
using Core.Application.Queries;
using Core.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Identity;

namespace Core.Infrastructure.Services;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedResult<UserDto>>
{
    private readonly ApplicationDbContext _context;

    public GetUsersQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(u => 
                u.FirstName!.ToLower().Contains(searchTerm) ||
                u.LastName!.ToLower().Contains(searchTerm) ||
                u.Email!.ToLower().Contains(searchTerm));
        }

        // Apply active filter
        if (request.IsActive.HasValue)
        {
            query = query.Where(u => request.IsActive.Value ? u.LockoutEnd == null : u.LockoutEnd != null);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDto
            {
                Id = Guid.Parse(u.Id),
                FirstName = u.FirstName ?? "",
                LastName = u.LastName ?? "",
                Email = u.Email ?? "",
                ProfilePictureUrl = u.AvatarUrl,
                IsActive = u.LockoutEnd == null,
                CreatedAt = u.CreatedAt,
                LastLoginAt = null // This would need to be tracked separately
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<UserDto>
        {
            Items = users,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
