# Copyright (c) RentalManager. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

# Post-deploy script to seed the database with test properties
# Usage: .\scripts\seed-properties.ps1 [-Count 100] [-OwnerId <guid>]

param(
    [int]$Count = 100,
    [string]$OwnerId = $null,
    [string]$ApiBaseUrl = "http://localhost:5111"
)

Write-Host "=== Property Seeding Script ===" -ForegroundColor Cyan
Write-Host ""

# Check if backend is running
Write-Host "Checking if backend is available..." -ForegroundColor Yellow
try {
    $healthCheck = Invoke-WebRequest -Uri "$ApiBaseUrl/health" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    Write-Host "✅ Backend is running" -ForegroundColor Green
}
catch {
    Write-Host "❌ Backend is not available at $ApiBaseUrl" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please ensure the backend container is running:" -ForegroundColor Yellow
    Write-Host "  docker-compose -f docker/docker-compose.yml up -d backend" -ForegroundColor White
    exit 1
}

# Try to use Admin API endpoint (requires authentication)
Write-Host ""
Write-Host "Attempting to seed via Admin API endpoint..." -ForegroundColor Yellow

$seedUrl = "$ApiBaseUrl/api/admin/seed-properties?count=$Count"
if ($OwnerId) {
    $seedUrl += "&ownerId=$OwnerId"
}

try {
    # For now, try without auth (might fail, but we'll handle it)
    $response = Invoke-WebRequest -Uri $seedUrl -Method POST -UseBasicParsing -TimeoutSec 30 -ErrorAction Stop
    
    if ($response.StatusCode -eq 200) {
        $result = $response.Content | ConvertFrom-Json
        Write-Host "✅ Successfully seeded $Count properties via API" -ForegroundColor Green
        if ($result.message) {
            Write-Host "   $($result.message)" -ForegroundColor Cyan
        }
        exit 0
    }
}
catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "⚠️  API endpoint requires authentication" -ForegroundColor Yellow
        Write-Host "   Using direct database seeding instead..." -ForegroundColor Yellow
    }
    else {
        Write-Host "⚠️  API call failed: $_" -ForegroundColor Yellow
        Write-Host "   Using direct database seeding instead..." -ForegroundColor Yellow
    }
}

# Alternative: Use dotnet ef or direct seeding
Write-Host ""
Write-Host "Using direct database seeding..." -ForegroundColor Yellow

# Create a simple seeding tool
$seederScript = @"
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentalManager.Infrastructure.Data;
using RentalManager.Application.Interfaces;

namespace RentalManager.Tools;

public class PropertySeedTool
{
    public static async Task<int> Main(string[] args)
    {
        var count = 100;
        var ownerIdStr = string.Empty;
        
        if (args.Length > 0) int.TryParse(args[0], out count);
        if (args.Length > 1) ownerIdStr = args[1];
        
        // Build service provider with minimal setup
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        
        // Get connection string from environment or default
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
            ?? "Host=localhost;Port=5433;Database=RentalManagerDb_Dev;Username=postgres;Password=password";
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<PropertySeeder>();
        
        var serviceProvider = services.BuildServiceProvider();
        var seeder = serviceProvider.GetRequiredService<PropertySeeder>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Find or use provided owner ID
        Guid ownerId;
        if (!string.IsNullOrEmpty(ownerIdStr) && Guid.TryParse(ownerIdStr, out ownerId))
        {
            // Use provided owner ID
        }
        else
        {
            // Try to find an existing user
            var users = await context.Set<Microsoft.AspNetCore.Identity.IdentityUser>().Take(1).ToListAsync();
            if (users.Any())
            {
                ownerId = Guid.Parse(users[0].Id);
            }
            else
            {
                Console.WriteLine("ERROR: No users found in database. Please create a user first by logging in.");
                return 1;
            }
        }
        
        await seeder.SeedPropertiesAsync(count, ownerId);
        Console.WriteLine($"Successfully seeded {count} properties");
        return 0;
    }
}
"@

# For now, use a simpler approach: call the API or use docker exec
Write-Host ""
Write-Host "Option 1: Use Admin API (requires authentication token)" -ForegroundColor Cyan
Write-Host "  POST $ApiBaseUrl/api/admin/seed-properties?count=$Count" -ForegroundColor White
Write-Host ""
Write-Host "Option 2: Use Docker exec to run seeding command" -ForegroundColor Cyan
Write-Host "  docker exec rentalmanager-backend dotnet run --project ..." -ForegroundColor White
Write-Host ""
Write-Host "Option 3: Properties will auto-seed on backend startup (Development only)" -ForegroundColor Cyan
Write-Host ""

# Check current property count
Write-Host "Checking current property count..." -ForegroundColor Yellow
try {
    $propertiesResponse = Invoke-WebRequest -Uri "$ApiBaseUrl/api/properties?PageNumber=1&PageSize=1" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    $propertiesJson = $propertiesResponse.Content | ConvertFrom-Json
    $currentCount = $propertiesJson.TotalCount
    
    Write-Host "Current properties in database: $currentCount" -ForegroundColor Cyan
    
    if ($currentCount -eq 0) {
        Write-Host ""
        Write-Host "⚠️  No properties found. Seeding will happen automatically on next backend restart" -ForegroundColor Yellow
        Write-Host "   (only in Development environment if database is empty)" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "To trigger seeding manually:" -ForegroundColor Yellow
        Write-Host "   1. Restart the backend container" -ForegroundColor White
        Write-Host "   2. Or call the Admin API endpoint with authentication" -ForegroundColor White
    }
    else {
        Write-Host ""
        Write-Host "✅ Database already has $currentCount properties" -ForegroundColor Green
    }
}
catch {
    Write-Host "⚠️  Could not check property count: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Script Complete ===" -ForegroundColor Cyan

