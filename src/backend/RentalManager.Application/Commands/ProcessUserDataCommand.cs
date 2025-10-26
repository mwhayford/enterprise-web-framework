// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using MediatR;

namespace RentalManager.Application.Commands;

public record ProcessUserDataCommand : IRequest
{
    public Guid UserId { get; init; }
}
