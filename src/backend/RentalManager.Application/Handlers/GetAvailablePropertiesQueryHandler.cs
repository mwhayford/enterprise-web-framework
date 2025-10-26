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
        var query = _context.Properties
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
            query = query.Where(p => p.Address.City.Contains(request.City));
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
            query = query.Where(p =>
                p.Description.Contains(request.SearchTerm) ||
                p.Address.Street.Contains(request.SearchTerm) ||
                p.Address.City.Contains(request.SearchTerm));
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

        var properties = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PropertyListDto
            {
                Id = p.Id,
                Address = p.Address.FullAddress,
                PropertyType = p.PropertyType,
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                SquareFeet = p.SquareFeet,
                MonthlyRent = p.MonthlyRent.Amount,
                RentCurrency = p.MonthlyRent.Currency,
                AvailableDate = p.AvailableDate,
                Status = p.Status,
                ThumbnailImage = p.Images.FirstOrDefault(),
                ApplicationFee = p.ApplicationFee != null ? p.ApplicationFee.Amount : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PropertyListDto>
        {
            Items = properties,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

