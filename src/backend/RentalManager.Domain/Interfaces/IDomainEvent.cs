// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Domain.Interfaces;

public interface IDomainEvent
{
    Guid Id { get; }

    DateTime OccurredOn { get; }
}
