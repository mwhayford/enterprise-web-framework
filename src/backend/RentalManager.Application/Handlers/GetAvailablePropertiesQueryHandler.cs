// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Queries;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.Handlers;

public class GetAvailablePropertiesQueryHandler : IRequestHandler<GetAvailablePropertiesQuery, PagedResult<PropertyListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailablePropertiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PropertyListDto>> Handle(GetAvailablePropertiesQuery request, CancellationToken cancellationToken)
    {
        // Validate pagination parameters
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Max(1, Math.Min(100, request.PageSize)); // Limit to 100 items per page

        var query = _context.Properties
            .AsNoTracking()
            .Where(p => p.Status == PropertyStatus.Available)
            .AsQueryable();

        // Apply filters
        if (request.MinBedrooms.HasValue)
        {
            query = query.Where(p => p.Bedrooms >= request.MinBedrooms.Value);
        }

        if (request.MaxBedrooms.HasValue)
        {
            query = query.Where(p => p.Bedrooms <= request.MaxBedrooms.Value);
        }

        if (request.MinBathrooms.HasValue)
        {
            query = query.Where(p => p.Bathrooms >= request.MinBathrooms.Value);
        }

        if (request.MaxBathrooms.HasValue)
        {
            query = query.Where(p => p.Bathrooms <= request.MaxBathrooms.Value);
        }

        if (request.MinRent.HasValue)
        {
            query = query.Where(p => p.MonthlyRent.Amount >= request.MinRent.Value);
        }

        if (request.MaxRent.HasValue)
        {
            query = query.Where(p => p.MonthlyRent.Amount <= request.MaxRent.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var cityLower = request.City.ToLowerInvariant();
            query = query.Where(p => p.Address.City.ToLower().Contains(cityLower));
        }

        if (!string.IsNullOrWhiteSpace(request.State))
        {
            query = query.Where(p => p.Address.State == request.State);
        }

        if (request.PropertyType.HasValue)
        {
            query = query.Where(p => p.PropertyType == request.PropertyType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLowerInvariant();
            query = query.Where(p =>
                p.Description.ToLower().Contains(searchTermLower) ||
                p.Address.Street.ToLower().Contains(searchTermLower) ||
                p.Address.City.ToLower().Contains(searchTermLower));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "rent" => request.SortDescending
                ? query.OrderByDescending(p => p.MonthlyRent.Amount)
                : query.OrderBy(p => p.MonthlyRent.Amount),
            "bedrooms" => request.SortDescending
                ? query.OrderByDescending(p => p.Bedrooms)
                : query.OrderBy(p => p.Bedrooms),
            "squarefeet" => request.SortDescending
                ? query.OrderByDescending(p => p.SquareFeet)
                : query.OrderBy(p => p.SquareFeet),
            "availabledate" => request.SortDescending
                ? query.OrderByDescending(p => p.AvailableDate)
                : query.OrderBy(p => p.AvailableDate),
            _ => request.SortDescending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        // CRITICAL FIX: Materialize the Property entities FIRST (including owned entities)
        // Then project to DTO in memory to avoid EF Core owned entity tracking issues
        // Accessing p.Address.Street even in Select() requires EF Core to materialize the owned entity
        // By materializing Property entities first, we avoid the tracking error
        var propertyEntities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Now project to DTO in memory (owned entities are already materialized)
        var properties = propertyEntities.Select(p => new PropertyListDto
        {
            Id = p.Id,

            // Construct FullAddress from owned entity (now in memory, safe to access)
            Address = p.Address.Unit != null
                ? $"{p.Address.Street}, {p.Address.Unit}, {p.Address.City}, {p.Address.State} {p.Address.ZipCode}, {p.Address.Country}"
                : $"{p.Address.Street}, {p.Address.City}, {p.Address.State} {p.Address.ZipCode}, {p.Address.Country}",
            PropertyType = p.PropertyType,
            Bedrooms = p.Bedrooms,
            Bathrooms = p.Bathrooms,
            SquareFeet = p.SquareFeet,
            MonthlyRent = p.MonthlyRent.Amount,
            RentCurrency = p.MonthlyRent.Currency,
            AvailableDate = p.AvailableDate,
            Status = p.Status,

            // Get first image from the collection (now in memory, safe to use LINQ)
            ThumbnailImage = p.Images.FirstOrDefault(),
            ApplicationFee = p.ApplicationFee != null ? p.ApplicationFee.Amount : null
        }).ToList();

        return new PagedResult<PropertyListDto>
        {
            Items = properties,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
