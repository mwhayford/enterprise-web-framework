// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Application.Interfaces;

public interface IDateTime
{
    DateTime Now { get; }

    DateTime UtcNow { get; }
}
