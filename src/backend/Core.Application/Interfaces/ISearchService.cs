// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Application.DTOs;

namespace Core.Application.Interfaces;

public interface ISearchService
{
    Task<SearchResultDto<T>> SearchAsync<T>(
        string query, 
        string index, 
        int page = 1, 
        int pageSize = 20,
        Dictionary<string, object>? filters = null) where T : class;

    Task IndexDocumentAsync<T>(T document, string index, string? id = null) where T : class;
    Task IndexDocumentsAsync<T>(IEnumerable<T> documents, string index) where T : class;
    Task DeleteDocumentAsync(string index, string id);
    Task DeleteIndexAsync(string index);
    Task<bool> IndexExistsAsync(string index);
    Task CreateIndexAsync(string index, object? mapping = null);
    Task UpdateDocumentAsync<T>(T document, string index, string id) where T : class;
}
