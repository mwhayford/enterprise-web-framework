// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;
using RentalManager.Application.DTOs;
using RentalManager.Domain.ValueObjects;

namespace RentalManager.Application.Queries;

public record GetAvailablePropertiesQuery(
    int PageNumber = 1,
    int PageSize = 20,
    int? MinBedrooms = null,
    int? MaxBedrooms = null,
    int? MinBathrooms = null,
    int? MaxBathrooms = null,
    decimal? MinRent = null,
    decimal? MaxRent = null,
    string? City = null,
    string? State = null,
    PropertyType? PropertyType = null,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false) : IRequest<PagedResult<PropertyListDto>>;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();

    public int TotalCount { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
