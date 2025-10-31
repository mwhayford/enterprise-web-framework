// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Domain.ValueObjects;

public enum ApplicationStatus
{
    Pending = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4,
    Withdrawn = 5
}

