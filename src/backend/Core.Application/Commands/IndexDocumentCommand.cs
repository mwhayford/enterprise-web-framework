// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;

namespace Core.Application.Commands;

public record IndexDocumentCommand<T> : IRequest
    where T : class
{
    public T Document { get; init; } = default!;

    public string Index { get; init; } = string.Empty;

    public string? Id { get; init; }
}
