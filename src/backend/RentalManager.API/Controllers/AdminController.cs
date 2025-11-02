// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManager.Application.Interfaces;
using RentalManager.Domain.Constants;
using RentalManager.Infrastructure.Data;

namespace RentalManager.API.Controllers;

/// <summary>
/// Controller for admin operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly PropertySeeder _propertySeeder;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        PropertySeeder propertySeeder,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<AdminController> logger)
    {
        _propertySeeder = propertySeeder;
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the database with test properties.
    /// </summary>
    /// <param name="count">Number of properties to seed (when byMetro=false).</param>
    /// <param name="ownerId">Optional owner ID. If not provided, uses the current user's ID.</param>
    /// <param name="byMetro">If true, seeds properties distributed across all major US metro areas.</param>
    /// <param name="propertiesPerMetro">Number of properties per metro area when byMetro is true (default: 1,000).</param>
    /// <returns>Result of the seeding operation.</returns>
    [HttpPost("seed-properties")]
    public async Task<ActionResult> SeedProperties(
        [FromQuery] int count = 1000,
        [FromQuery] Guid? ownerId = null,
        [FromQuery] bool byMetro = false,
        [FromQuery] int? propertiesPerMetro = null)
    {
        var targetOwnerId = ownerId ?? _currentUserService.UserId;
        if (!targetOwnerId.HasValue)
        {
            return Unauthorized(new { error = "Owner ID is required" });
        }

        try
        {
            int seededCount;
            int finalCount;
            string message;

            if (byMetro || propertiesPerMetro.HasValue)
            {
                // Seed by metro area distribution (1,000 per metro across all states)
                var perMetro = propertiesPerMetro ?? 1000;
                if (perMetro < 1 || perMetro > 5000)
                {
                    return BadRequest(new { error = "Properties per metro must be between 1 and 5000" });
                }

                _logger.LogInformation("Seeding properties by metro area: {PropertiesPerMetro} per metro area", perMetro);
                seededCount = await _propertySeeder.SeedPropertiesByMetroAreaAsync(perMetro, targetOwnerId.Value);
                finalCount = await _context.Properties.CountAsync();
                message = $"Successfully seeded {seededCount} properties across all major US metro areas ({perMetro} per metro).";
            }
            else
            {
                // Traditional seeding: check existing count and seed difference
                var existingCount = await _context.Properties.CountAsync();

                if (count < 1 || count > 100000)
                {
                    return BadRequest(new { error = "Count must be between 1 and 100000" });
                }

                if (existingCount >= count)
                {
                    _logger.LogInformation("Database already has {ExistingCount} properties (target: {Count}). Skipping seed to avoid duplicates.", existingCount, count);
                    return Ok(new
                    {
                        message = $"Database already has {existingCount} properties (target: {count}). No seeding needed.",
                        existingCount = existingCount,
                        targetCount = count,
                        seededCount = 0,
                        ownerId = targetOwnerId.Value
                    });
                }

                // Only seed the difference to reach the target count
                var propertiesToSeed = count - existingCount;
                _logger.LogInformation("Database has {ExistingCount} properties. Seeding {Count} additional properties to reach target of {TargetCount}.", existingCount, propertiesToSeed, count);

                await _propertySeeder.SeedPropertiesAsync(propertiesToSeed, targetOwnerId.Value);
                seededCount = propertiesToSeed;
                finalCount = await _context.Properties.CountAsync();
                message = $"Successfully seeded {seededCount} properties. Total properties: {finalCount}";
            }

            _logger.LogInformation("Completed property seed operation. Total properties: {TotalCount}", finalCount);

            return Ok(new
            {
                message = message,
                seededCount = seededCount,
                totalCount = finalCount,
                ownerId = targetOwnerId.Value,
                byMetro = byMetro || propertiesPerMetro.HasValue
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding properties");
            return StatusCode(500, new { error = "Failed to seed properties", details = ex.Message });
        }
    }
}
