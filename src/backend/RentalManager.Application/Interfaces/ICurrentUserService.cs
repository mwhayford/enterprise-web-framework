// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
namespace RentalManager.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

    IEnumerable<string> Roles { get; }

    bool IsInRole(string role);
}
