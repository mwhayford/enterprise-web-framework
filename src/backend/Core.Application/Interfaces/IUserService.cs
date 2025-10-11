// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Domain.Entities;
using Core.Domain.ValueObjects;

namespace Core.Application.Interfaces;

public interface IUserService
{
    Task<User> RegisterUserAsync(
        string firstName,
        string lastName,
        Email email,
        string? googleId = null,
        string? profilePictureUrl = null);

    Task<User?> GetUserByIdAsync(Guid userId);

    Task<User?> GetUserByEmailAsync(Email email);

    Task<User?> GetUserByGoogleIdAsync(string googleId);

    Task<User> UpdateUserProfileAsync(Guid userId, string firstName, string lastName, string? profilePictureUrl = null);

    Task<IEnumerable<User>> GetUsersAsync(int page = 1, int pageSize = 20);

    Task<bool> DeactivateUserAsync(Guid userId);

    Task<bool> ActivateUserAsync(Guid userId);
}
