// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Domain.ValueObjects;

public enum LeaseStatus
{
    Draft = 1,
    Active = 2,
    Expired = 3,
    Terminated = 4,
    Renewed = 5
}

