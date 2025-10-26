// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;

namespace RentalManager.Application.Commands;

public record ActivateUserCommand : IRequest<bool>
{
    public Guid UserId { get; init; }
}
