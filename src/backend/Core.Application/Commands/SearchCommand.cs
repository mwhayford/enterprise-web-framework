// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Application.DTOs;
using MediatR;

namespace Core.Application.Commands;

public record SearchCommand<T> : IRequest<SearchResultDto<T>>
    where T : class
{
    public string Query { get; init; } = string.Empty;

    public string Index { get; init; } = string.Empty;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public Dictionary<string, object>? Filters { get; init; }
}
