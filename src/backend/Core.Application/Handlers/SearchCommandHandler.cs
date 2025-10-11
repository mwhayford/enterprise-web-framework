using MediatR;
using Core.Application.Commands;
using Core.Application.DTOs;
using Core.Application.Interfaces;

namespace Core.Application.Handlers;

public class SearchCommandHandler<T> : IRequestHandler<SearchCommand<T>, SearchResultDto<T>> where T : class
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
