// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Application.Commands;
using Core.Application.Interfaces;
using MediatR;

namespace Core.Application.Handlers;

public class IndexDocumentCommandHandler<T> : IRequestHandler<IndexDocumentCommand<T>>
    where T : class
{
    private readonly ISearchService _searchService;

    public IndexDocumentCommandHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task Handle(IndexDocumentCommand<T> request, CancellationToken cancellationToken)
    {
        await _searchService.IndexDocumentAsync(request.Document, request.Index, request.Id);
    }
}
