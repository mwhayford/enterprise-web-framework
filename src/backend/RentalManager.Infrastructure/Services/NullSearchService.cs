// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;

namespace RentalManager.Infrastructure.Services;

/// <summary>
/// No-op implementation of ISearchService used when Elasticsearch is not available (e.g., in tests or CI).
/// All methods log and return empty/default values without actually performing search operations.
/// </summary>
public class NullSearchService : ISearchService
{
    private readonly ILogger<NullSearchService> _logger;

    public NullSearchService(ILogger<NullSearchService> logger)
    {
        _logger = logger;
    }

    public Task<SearchResultDto<T>> SearchAsync<T>(
        string query,
        string index,
        int page = 1,
        int pageSize = 20,
        Dictionary<string, object>? filters = null)
        where T : class
    {
        _logger.LogDebug("Search service is not available. Search query '{Query}' for index '{Index}' will return empty results.", query, index);
        return Task.FromResult(new SearchResultDto<T>
        {
            Documents = new List<T>(),
            TotalHits = 0,
            Page = page,
            PageSize = pageSize,
            MaxScore = 0,
            Took = TimeSpan.Zero,
            Aggregations = new Dictionary<string, object>()
        });
    }

    public Task IndexDocumentAsync<T>(T document, string index, string? id = null)
        where T : class
    {
        _logger.LogDebug("Search service is not available. Document indexing for index '{Index}' will be ignored.", index);
        return Task.CompletedTask;
    }

    public Task IndexDocumentsAsync<T>(IEnumerable<T> documents, string index)
        where T : class
        _logger.LogDebug("Search service is not available. Batch document indexing for index '{Index}' will be ignored.", index);
        return Task.CompletedTask;
    }

    public Task DeleteDocumentAsync(string index, string id)
    {
        _logger.LogDebug("Search service is not available. Document deletion for index '{Index}', id '{Id}' will be ignored.", index, id);
        return Task.CompletedTask;
    }

    public Task DeleteIndexAsync(string index)
    {
        _logger.LogDebug("Search service is not available. Index deletion for '{Index}' will be ignored.", index);
        return Task.CompletedTask;
    }

    public Task<bool> IndexExistsAsync(string index)
    {
        _logger.LogDebug("Search service is not available. Index existence check for '{Index}' will return false.", index);
        return Task.FromResult(false);
    }

    public Task CreateIndexAsync(string index, object? mapping = null)
    {
        _logger.LogDebug("Search service is not available. Index creation for '{Index}' will be ignored.", index);
        return Task.CompletedTask;
    }

    public Task UpdateDocumentAsync<T>(T document, string index, string id)
        where T : class
    {
        _logger.LogDebug("Search service is not available. Document update for index '{Index}', id '{Id}' will be ignored.", index, id);
        return Task.CompletedTask;
    }
}

