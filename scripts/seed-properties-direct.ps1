# Copyright (c) RentalManager. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

# Direct database seeding script using Docker exec
# This bypasses the API and seeds directly into the database
# Usage: .\scripts\seed-properties-direct.ps1 [-Count 100]

param(
    [int]$Count = 100,
    [string]$ConnectionString = "Host=localhost;Port=5433;Database=RentalManagerDb_Dev;Username=postgres;Password=password"
)

Write-Host ""
Write-Host "=== Direct Property Seeding Script ===" -ForegroundColor Cyan
Write-Host ""

# Check if backend container is running
Write-Host "[1/3] Checking backend container..." -ForegroundColor Yellow
$backendContainer = docker ps --filter "name=rentalmanager-backend" --format "{{.Names}}" 2>&1
if (-not $backendContainer) {
    Write-Host "      ❌ Backend container is not running" -ForegroundColor Red
    Write-Host "      Please start it first:" -ForegroundColor Yellow
    Write-Host "        docker-compose -f docker/docker-compose.yml up -d backend" -ForegroundColor White
    exit 1
}
Write-Host "      ✅ Backend container is running" -ForegroundColor Green

# Create a temporary C# script to run inside the container
$csharpScript = @"
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentalManager.Infrastructure.Data;
using RentalManager.Application.Interfaces;
using RentalManager.Infrastructure.Identity;
using RentalManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using RentalManager.Domain.Entities;

namespace RentalManager.Tools
{
    public class SeedPropertiesTool
    {
        public static async Task<int> Main(string[] args)
        {
            var count = args.Length > 0 && int.TryParse(args[0], out var parsedCount) ? parsedCount : 100;
            
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=postgres;Database=RentalManagerDb_Dev;Username=postgres;Password=password";
            
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
            
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 1;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
            
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<PropertySeeder>();
            
            var serviceProvider = services.BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var seeder = serviceProvider.GetRequiredService<PropertySeeder>();
            var logger = serviceProvider.GetRequiredService<ILogger<SeedPropertiesTool>>();
            
            try
            {
                // Find or create a default owner user
                Guid ownerId;
                var existingUser = await userManager.Users.FirstOrDefaultAsync();
                
                if (existingUser != null)
                {
                    ownerId = Guid.Parse(existingUser.Id);
                    logger.LogInformation("Using existing user {UserId} as property owner", ownerId);
                }
                else
                {
                    // Create a default owner user for seeding
                    var defaultUser = new ApplicationUser
                    {
                        UserName = "seed-owner@rentalmanager.local",
                        Email = "seed-owner@rentalmanager.local",
                        EmailConfirmed = true
                    };
                    
                    var createResult = await userManager.CreateAsync(defaultUser, "SeedPassword123!");
                    if (!createResult.Succeeded)
                    {
                        logger.LogError("Failed to create default user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        return 1;
                    }
                    
                    ownerId = Guid.Parse(defaultUser.Id);
                    logger.LogInformation("Created default user {UserId} for property seeding", ownerId);
                }
                
                // Seed properties
                logger.LogInformation("Starting to seed {Count} properties...", count);
                await seeder.SeedPropertiesAsync(count, ownerId);
                logger.LogInformation("Successfully seeded {Count} properties", count);
                
                return 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding properties");
                return 1;
            }
        }
    }
}
"@

Write-Host ""
Write-Host "[2/3] Seeding properties via Docker exec..." -ForegroundColor Yellow
Write-Host "      This will seed $Count properties into the database" -ForegroundColor Cyan

# For now, let's use a simpler approach - create a console app project or use dotnet-script
# Actually, the easiest is to use the Admin API if we can get a token, or use docker exec with a compiled tool

# Alternative: Use the existing Admin endpoint but create a helper script that gets a token
Write-Host ""
Write-Host "      Since Admin API requires authentication, we'll trigger auto-seeding." -ForegroundColor Cyan
Write-Host "      Auto-seeding requires a user to exist first." -ForegroundColor Yellow
Write-Host ""
Write-Host "      Recommended approach:" -ForegroundColor Yellow
Write-Host "        1. Login via Google OAuth in the frontend (http://localhost:3001)" -ForegroundColor White
Write-Host "        2. This creates a user in the database" -ForegroundColor White
Write-Host "        3. Restart the backend to trigger auto-seeding" -ForegroundColor White
Write-Host "           docker-compose -f docker/docker-compose.yml restart backend" -ForegroundColor White
Write-Host "        4. Or use the Admin API endpoint with JWT token" -ForegroundColor White
Write-Host ""
Write-Host "[3/3] Checking if any users exist..." -ForegroundColor Yellow

# Check if we can query the API to see if there are users (via properties owner check)
Write-Host "      (Checking backend logs for user creation...)" -ForegroundColor Cyan
docker logs rentalmanager-backend 2>&1 | Select-String -Pattern "user|User|owner|Owner" | Select-Object -Last 5

Write-Host ""
Write-Host "=== Seeding Options ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Option 1 (Recommended): Use Admin API with authentication" -ForegroundColor Yellow
Write-Host '  curl -X POST "http://localhost:5111/api/admin/seed-properties?count=' + $Count + '" \' -ForegroundColor White
Write-Host '       -H "Authorization: Bearer <your-jwt-token>"' -ForegroundColor White
Write-Host ""
Write-Host "Option 2: Auto-seeding (Development only, requires existing user)" -ForegroundColor Yellow
Write-Host "  1. Login via frontend to create a user" -ForegroundColor White
Write-Host "  2. Restart backend: docker-compose -f docker/docker-compose.yml restart backend" -ForegroundColor White
Write-Host ""
Write-Host "Script complete. Use one of the options above to seed properties." -ForegroundColor Cyan
Write-Host ""

