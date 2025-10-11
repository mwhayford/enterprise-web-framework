// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Core.Application.Interfaces;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Domain.Events;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Identity;
using MediatR;
using Core.Application.Commands;

namespace Core.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventPublisher _eventPublisher;
    private readonly IMediator _mediator;
    private readonly IMetricsService _metricsService;

    public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEventPublisher eventPublisher, IMediator mediator, IMetricsService metricsService)
    {
        _context = context;
        _userManager = userManager;
        _eventPublisher = eventPublisher;
        _mediator = mediator;
        _metricsService = metricsService;
    }

    public async Task<User> RegisterUserAsync(
        string firstName,
        string lastName,
        Email email,
        string? googleId = null,
        string? profilePictureUrl = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(email.Value);
        if (existingUser != null)
        {
            // Update existing user with Google info if not already set
            if (!string.IsNullOrEmpty(googleId) && string.IsNullOrEmpty(existingUser.GoogleId))
            {
                existingUser.GoogleId = googleId;
                existingUser.FirstName = firstName;
                existingUser.LastName = lastName;
                existingUser.AvatarUrl = profilePictureUrl;
                existingUser.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(existingUser);
            }
            
            // Convert ApplicationUser to Domain User
            return new User(
                existingUser.FirstName ?? firstName,
                existingUser.LastName ?? lastName,
                email,
                existingUser.GoogleId,
                existingUser.AvatarUrl);
        }

        // Create new ApplicationUser
        var applicationUser = new ApplicationUser
        {
            UserName = email.Value,
            Email = email.Value,
            FirstName = firstName,
            LastName = lastName,
            GoogleId = googleId,
            AvatarUrl = profilePictureUrl,
            EmailConfirmed = true, // Google OAuth users are pre-verified
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(applicationUser);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Convert ApplicationUser to Domain User
        var domainUser = new User(
            applicationUser.FirstName ?? firstName,
            applicationUser.LastName ?? lastName,
            email,
            applicationUser.GoogleId,
            applicationUser.AvatarUrl);

        // Publish user created event
        await _eventPublisher.PublishAsync(new UserCreatedEvent
        {
            UserId = domainUser.Id,
            Email = domainUser.Email.Value,
            FirstName = domainUser.FirstName,
            LastName = domainUser.LastName
        });

        // Queue welcome email as background job
        await _mediator.Send(new SendWelcomeEmailCommand
        {
            Email = domainUser.Email.Value,
            FirstName = domainUser.FirstName
        });

        // Queue user data processing as background job
        await _mediator.Send(new ProcessUserDataCommand
        {
            UserId = domainUser.Id
        });

        // Record metrics
        stopwatch.Stop();
        _metricsService.RecordUserRegistration();
        _metricsService.RecordUserRegistrationTime(stopwatch.Elapsed);

        return domainUser;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        var applicationUser = await _userManager.FindByIdAsync(userId.ToString());
        if (applicationUser == null) return null;

        return ConvertToDomainUser(applicationUser);
    }

    public async Task<User?> GetUserByEmailAsync(Email email)
    {
        var applicationUser = await _userManager.FindByEmailAsync(email.Value);
        if (applicationUser == null) return null;

        return ConvertToDomainUser(applicationUser);
    }

    public async Task<User?> GetUserByGoogleIdAsync(string googleId)
    {
        var applicationUser = await _context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleId);
        if (applicationUser == null) return null;

        return ConvertToDomainUser(applicationUser);
    }

    public async Task<User> UpdateUserProfileAsync(Guid userId, string firstName, string lastName, string? profilePictureUrl = null)
    {
        var applicationUser = await _userManager.FindByIdAsync(userId.ToString());
        if (applicationUser == null)
            throw new ArgumentException("User not found", nameof(userId));

        applicationUser.FirstName = firstName;
        applicationUser.LastName = lastName;
        applicationUser.AvatarUrl = profilePictureUrl;
        applicationUser.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(applicationUser);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        return ConvertToDomainUser(applicationUser);
    }

    public async Task<IEnumerable<User>> GetUsersAsync(int page = 1, int pageSize = 20)
    {
        var applicationUsers = await _context.Users
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return applicationUsers.Select(ConvertToDomainUser);
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var applicationUser = await _userManager.FindByIdAsync(userId.ToString());
        if (applicationUser == null)
            return false;

        applicationUser.LockoutEnabled = true;
        applicationUser.LockoutEnd = DateTimeOffset.MaxValue;
        applicationUser.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(applicationUser);
        return result.Succeeded;
    }

    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        var applicationUser = await _userManager.FindByIdAsync(userId.ToString());
        if (applicationUser == null)
            return false;

        applicationUser.LockoutEnabled = false;
        applicationUser.LockoutEnd = null;
        applicationUser.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(applicationUser);
        return result.Succeeded;
    }

    private User ConvertToDomainUser(ApplicationUser applicationUser)
    {
        return new User(
            applicationUser.FirstName ?? "",
            applicationUser.LastName ?? "",
            Core.Domain.ValueObjects.Email.Create(applicationUser.Email!),
            applicationUser.GoogleId,
            applicationUser.AvatarUrl);
    }
}
