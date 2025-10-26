// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.Identity;

namespace RentalManager.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? GoogleId { get; set; }

    public string? StripeCustomerId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
