// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Application.Commands;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using MediatR;

namespace RentalManager.Application.Handlers;

public class SearchCommandHandler<T> : IRequestHandler<SearchCommand<T>, SearchResultDto<T>>
    where T : class
{
    private readonly ISearchService _searchService;

    public SearchCommandHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<SearchResultDto<T>> Handle(SearchCommand<T> request, CancellationToken cancellationToken)
    {
        return await _searchService.SearchAsync<T>(
            request.Query,
            request.Index,
            request.Page,
            request.PageSize,
            request.Filters);
    }
}
