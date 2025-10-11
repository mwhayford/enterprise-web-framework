// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace Core.Application.DTOs;

public record SearchResultDto<T>
{
    public IEnumerable<T> Documents { get; init; } = Enumerable.Empty<T>();
    public long TotalHits { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalHits / PageSize);
    public double MaxScore { get; init; }
    public TimeSpan Took { get; init; }
    public Dictionary<string, object> Aggregations { get; init; } = new();
}
