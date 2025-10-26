// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Domain.ValueObjects;

public enum SubscriptionStatus
{
    Incomplete = 0,
    IncompleteExpired = 1,
    Trialing = 2,
    Active = 3,
    PastDue = 4,
    Canceled = 5,
    Unpaid = 6,
    Paused = 7
}
