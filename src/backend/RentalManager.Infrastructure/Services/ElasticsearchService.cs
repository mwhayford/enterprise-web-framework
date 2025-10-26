// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Application.DTOs;
using RentalManager.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace RentalManager.Infrastructure.Services;

public class ElasticsearchService : ISearchService
{
    private readonly IElasticClient _client;
    private readonly ILogger<ElasticsearchService> _logger;
    private readonly ElasticsearchSettings _settings;

    public ElasticsearchService(
        IElasticClient client,
        ILogger<ElasticsearchService> logger,
        IOptions<ElasticsearchSettings> settings)
    {
        _client = client;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<SearchResultDto<T>> SearchAsync<T>(
        string query,
        string index,
        int page = 1,
        int pageSize = 20,
        Dictionary<string, object>? filters = null)
        where T : class
    {
        try
        {
            var searchRequest = new SearchRequest<T>(index)
            {
                Query = BuildQuery(query, filters),
                From = (page - 1) * pageSize,
                Size = pageSize,
                Sort = new List<ISort>
                {
                    new FieldSort { Field = "_score", Order = SortOrder.Descending }
                }
            };

            var response = await _client.SearchAsync<T>(searchRequest);

            if (!response.IsValid)
            {
                _logger.LogError("Elasticsearch search failed: {Error}", response.ServerError?.Error?.Reason);
                throw new InvalidOperationException($"Search failed: {response.ServerError?.Error?.Reason}");
            }

            return new SearchResultDto<T>
            {
                Documents = response.Documents,
                TotalHits = response.Total,
                Page = page,
                PageSize = pageSize,
                MaxScore = response.MaxScore,
                Took = TimeSpan.FromMilliseconds(response.Took),
                Aggregations = response.Aggregations?.ToDictionary(a => a.Key, a => (object)a.Value) ?? new Dictionary<string, object>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching in Elasticsearch for query: {Query}", query);
            throw;
        }
    }

    public async Task IndexDocumentAsync<T>(T document, string index, string? id = null)
        where T : class
    {
        try
        {
            var response = await _client.IndexAsync(document, i => i
                .Index(index)
                .Id(id));

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index document: {Error}", response.ServerError?.Error?.Reason);
                throw new InvalidOperationException($"Indexing failed: {response.ServerError?.Error?.Reason}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document to Elasticsearch");
            throw;
        }
    }

    public async Task IndexDocumentsAsync<T>(IEnumerable<T> documents, string index)
        where T : class
    {
        try
        {
            var response = await _client.BulkAsync(b => b
                .Index(index)
                .IndexMany(documents));

            if (!response.IsValid)
            {
                _logger.LogError("Failed to bulk index documents: {Error}", response.ServerError?.Error?.Reason);
                throw new InvalidOperationException($"Bulk indexing failed: {response.ServerError?.Error?.Reason}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk indexing documents to Elasticsearch");
            throw;
        }
    }

    public async Task DeleteDocumentAsync(string index, string id)
    {
        try
        {
            var response = await _client.DeleteAsync<object>(id, d => d.Index(index));

            if (!response.IsValid)
            {
                _logger.LogError("Failed to delete document: {Error}", response.ServerError?.Error?.Reason);
                throw new InvalidOperationException($"Delete failed: {response.ServerError?.Error?.Reason}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document from Elasticsearch");
            throw;
        }
    }

    public async Task DeleteIndexAsync(string index)
    {
        try
        {
            var response = await _client.Indices.DeleteAsync(index);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to delete index: {Error}", response.ServerError?.Error?.Reason);
                throw new InvalidOperationException($"Index deletion failed: {response.ServerError?.Error?.Reason}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting index from Elasticsearch");
            throw;
        }
    }

    public async Task<bool> IndexExistsAsync(string index)
    {
        try
        {
            var response = await _client.Indices.ExistsAsync(index);
            return response.Exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if index exists in Elasticsearch");
            return false;
        }
    }

    public async Task CreateIndexAsync(string index, object? mapping = null)
    {
        try
        {
            var createIndexRequest = new CreateIndexRequest(index);

            if (mapping != null)
            {
                createIndexRequest.Mappings = new TypeMapping { Properties = (IProperties)mapping };
            }

            var response = await _client.Indices.CreateAsync(createIndexRequest);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to create index: {Error}", response.ServerError?.Error?.Reason);
                throw new InvalidOperationException($"Index creation failed: {response.ServerError?.Error?.Reason}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index in Elasticsearch");
            throw;
        }
    }

    public async Task UpdateDocumentAsync<T>(T document, string index, string id)
        where T : class
    {
        try
        {
            var response = await _client.UpdateAsync<T>(id, u => u
                .Index(index)
                .Doc(document));

            if (!response.IsValid)
            {
                _logger.LogError("Failed to update document: {Error}", response.ServerError?.Error?.Reason);
                throw new InvalidOperationException($"Update failed: {response.ServerError?.Error?.Reason}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document in Elasticsearch");
            throw;
        }
    }

    private QueryContainer BuildQuery(string query, Dictionary<string, object>? filters)
    {
        var queries = new List<QueryContainer>();

        // Add text search query
        if (!string.IsNullOrEmpty(query))
        {
            queries.Add(new MultiMatchQuery
            {
                Query = query,
                Fields = new Field[] { "*" },
                Type = TextQueryType.BestFields,
                Fuzziness = Fuzziness.Auto
            });
        }

        // Add filters
        if (filters != null && filters.Any())
        {
            foreach (var filter in filters)
            {
                queries.Add(new TermQuery
                {
                    Field = filter.Key,
                    Value = filter.Value
                });
            }
        }

        return queries.Any()
            ? new BoolQuery { Must = queries }
            : new MatchAllQuery();
    }
}

public class ElasticsearchSettings
{
    public string Url { get; set; } = "http://localhost:9200";

    public string DefaultIndex { get; set; } = "core-index";

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
