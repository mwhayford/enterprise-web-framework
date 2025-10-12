// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Core.Domain.Events;
using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

public class User : BaseEntity
{
    public User(
        string firstName,
        string lastName,
        Email email,
        string? googleId = null,
        string? profilePictureUrl = null)
        : base()
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be null or empty", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name cannot be null or empty", nameof(lastName));
        }

        ArgumentNullException.ThrowIfNull(email);

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        GoogleId = googleId;
        ProfilePictureUrl = profilePictureUrl;
        IsActive = true;

        AddDomainEvent(new UserCreatedEvent
        {
            UserId = Id,
            Email = email.Value,
            FirstName = firstName,
            LastName = lastName
        });
    }

    private User()
    {
        FirstName = default!;
        LastName = default!;
        Email = default!;
    } // For EF Core

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public Email Email { get; private set; }

    public string? GoogleId { get; private set; }

    public string? ProfilePictureUrl { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateProfile(string firstName, string lastName, string? profilePictureUrl = null)
    {
        FirstName = firstName;
        LastName = lastName;
        ProfilePictureUrl = profilePictureUrl;
        UpdateTimestamp();
    }

    public void SetGoogleId(string googleId)
    {
        GoogleId = googleId;
        UpdateTimestamp();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }
}
