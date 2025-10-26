// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using RentalManager.Domain.Events;

namespace RentalManager.Domain.Interfaces;

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void AddDomainEvent(IDomainEvent domainEvent);

    void RemoveDomainEvent(IDomainEvent domainEvent);

    void ClearDomainEvents();
}
