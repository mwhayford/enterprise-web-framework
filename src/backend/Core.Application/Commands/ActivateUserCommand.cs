// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;

namespace Core.Application.Commands;

public record ActivateUserCommand : IRequest<bool>
{
    public Guid UserId { get; init; }
}
