// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Application.DTOs;
using MediatR;

namespace Core.Application.Queries;

public record GetUsersQuery : IRequest<PaginatedResult<UserDto>>
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string? SearchTerm { get; init; }

    public bool? IsActive { get; init; }
}
