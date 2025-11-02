# Copyright (c) RentalManager. All rights reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

# Post-deployment script to seed the database with 100 test properties
# Usage: .\scripts\post-deploy-seed.ps1 [-PropertyCount 100] [-ApiBaseUrl "http://localhost:5111"] [-AuthToken "<jwt-token>"]

param(
    [int]$PropertyCount = 100,
    [string]$ApiBaseUrl = "http://localhost:5111",
    [string]$AuthToken = $null,
    [switch]$ForceRestart
)

$ErrorActionPreference = "Continue"

Write-Host ""
Write-Host "=== Post-Deploy Property Seeding Script ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Verify backend is running
Write-Host "[1/4] Checking backend availability..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-WebRequest -Uri "$ApiBaseUrl/health" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    Write-Host "      ✅ Backend is healthy" -ForegroundColor Green
}
catch {
    Write-Host "      ❌ Backend is not available at $ApiBaseUrl" -ForegroundColor Red
    Write-Host ""
    Write-Host "      Please start the backend:" -ForegroundColor Yellow
    Write-Host "        docker-compose -f docker/docker-compose.yml up -d backend" -ForegroundColor White
    exit 1
}

# Step 2: Check current property count
Write-Host ""
Write-Host "[2/4] Checking current property count..." -ForegroundColor Yellow
try {
    $propertiesResponse = Invoke-WebRequest -Uri "$ApiBaseUrl/api/properties?PageNumber=1&PageSize=1" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    $propertiesJson = $propertiesResponse.Content | ConvertFrom-Json
    $currentCount = $propertiesJson.TotalCount
    Write-Host "      Current properties: $currentCount" -ForegroundColor Cyan
    
    if ($currentCount -ge $PropertyCount) {
        Write-Host "      ✅ Database already has sufficient properties ($currentCount >= $PropertyCount)" -ForegroundColor Green
        Write-Host "      Skipping seed to avoid duplicates" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "=== Seeding Complete (No action needed) ===" -ForegroundColor Cyan
        exit 0
    }
    
    # Also check if properties exist but less than target
    if ($currentCount -gt 0) {
        Write-Host "      ⚠️  Database has $currentCount properties (target: $PropertyCount)" -ForegroundColor Yellow
        Write-Host "      To avoid duplicates, only missing properties will be seeded" -ForegroundColor Cyan
        $PropertyCount = [Math]::Max(1, $PropertyCount - $currentCount)
        Write-Host "      Will seed $PropertyCount additional properties" -ForegroundColor Cyan
    }
}
catch {
    Write-Host "      ⚠️  Could not retrieve property count: $_" -ForegroundColor Yellow
    $currentCount = 0
}

# Step 3: Attempt to seed via Admin API
Write-Host ""
Write-Host "[3/4] Attempting to seed properties..." -ForegroundColor Yellow

$seedUrl = "$ApiBaseUrl/api/admin/seed-properties?count=$PropertyCount"
$headers = @{
    "Content-Type" = "application/json"
}

if ($AuthToken) {
    $headers["Authorization"] = "Bearer $AuthToken"
    Write-Host "      Using provided authentication token" -ForegroundColor Cyan
}

$seeded = $false

try {
    $seedResponse = Invoke-WebRequest -Uri $seedUrl -Method POST -Headers $headers -UseBasicParsing -TimeoutSec 120 -ErrorAction Stop
    
    if ($seedResponse.StatusCode -eq 200) {
        $result = $seedResponse.Content | ConvertFrom-Json
        Write-Host "      ✅ Successfully seeded $PropertyCount properties via Admin API" -ForegroundColor Green
        if ($result.message) {
            Write-Host "      $($result.message)" -ForegroundColor Cyan
        }
        $seeded = $true
    }
}
catch {
    $statusCode = if ($_.Exception.Response) { $_.Exception.Response.StatusCode.value__ } else { $null }
    
    if ($statusCode -eq 401) {
        Write-Host "      ⚠️  Authentication required (401 Unauthorized)" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "      The Admin API endpoint requires admin authentication." -ForegroundColor Cyan
        Write-Host ""
        Write-Host "      Options:" -ForegroundColor Yellow
        Write-Host "      1. Login via Google OAuth in the frontend to create a user" -ForegroundColor White
        Write-Host "      2. Get a JWT token and run:" -ForegroundColor White
        Write-Host "         .\scripts\post-deploy-seed.ps1 -AuthToken `"<your-jwt-token>`"" -ForegroundColor White
        Write-Host "      3. Properties will auto-seed on backend restart (Development only)" -ForegroundColor White
    }
    elseif ($statusCode -eq 403) {
        Write-Host "      ⚠️  Access forbidden (403) - Admin role required" -ForegroundColor Yellow
    }
    else {
        Write-Host "      ⚠️  API call failed: $_" -ForegroundColor Yellow
        if ($statusCode) {
            Write-Host "      Status Code: $statusCode" -ForegroundColor Yellow
        }
    }
}

# Step 4: Alternative - Provide seeding instructions
if (-not $seeded) {
    Write-Host ""
    Write-Host "[4/4] Seeding instructions..." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "      ⚠️  Admin API requires authentication" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "      Choose one of these methods to seed properties:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "      Method 1: Use Admin API with JWT Token (Recommended)" -ForegroundColor Yellow
    Write-Host "        1. Login via Google OAuth: http://localhost:3001" -ForegroundColor White
    Write-Host "        2. Open browser DevTools (F12) > Application > Local Storage" -ForegroundColor White
    Write-Host "        3. Find 'authToken' or 'token' key and copy the JWT value" -ForegroundColor White
    Write-Host "        4. Run: .\scripts\post-deploy-seed.ps1 -AuthToken `"<your-token>`"" -ForegroundColor White
    Write-Host ""
    Write-Host "      Method 2: Auto-seeding on Backend Restart" -ForegroundColor Yellow
    Write-Host "        Prerequisites:" -ForegroundColor White
    Write-Host "          - Development environment (ASPNETCORE_ENVIRONMENT=Development)" -ForegroundColor White
    Write-Host "          - At least one user exists (login via frontend first)" -ForegroundColor White
    Write-Host "          - Database is empty (0 properties)" -ForegroundColor White
    Write-Host "        Steps:" -ForegroundColor White
    Write-Host "          1. Login via frontend to create a user" -ForegroundColor White
    Write-Host "          2. Run: docker-compose -f docker/docker-compose.yml restart backend" -ForegroundColor White
    Write-Host "          3. Backend will auto-seed 100 properties on startup" -ForegroundColor White
    Write-Host ""
    
    if ($ForceRestart) {
        Write-Host "      Attempting auto-seeding via restart..." -ForegroundColor Cyan
        docker-compose -f docker/docker-compose.yml restart backend 2>&1 | Out-Null
        Write-Host "      ✅ Backend restarted. Waiting for auto-seeding (20 seconds)..." -ForegroundColor Green
        Start-Sleep -Seconds 20
        
        # Check again
        try {
            $checkResponse = Invoke-WebRequest -Uri "$ApiBaseUrl/api/properties?PageNumber=1&PageSize=1" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
            $checkJson = $checkResponse.Content | ConvertFrom-Json
            $newCount = $checkJson.TotalCount
            
            if ($newCount -gt $currentCount) {
                Write-Host "      ✅ Auto-seeding completed! New property count: $newCount" -ForegroundColor Green
                $seeded = $true
            }
            else {
                Write-Host "      ⚠️  Auto-seeding did not trigger (count still $newCount)" -ForegroundColor Yellow
                Write-Host "      Reason: Requires Development env AND at least one user exists" -ForegroundColor Yellow
                Write-Host "      Solution: Login via frontend first, then restart backend" -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "      ⚠️  Could not verify seeding after restart: $_" -ForegroundColor Yellow
        }
    }
}

# Final verification
if ($seeded) {
    Write-Host ""
    Write-Host "=== Final Verification ===" -ForegroundColor Cyan
    Start-Sleep -Seconds 2
    
    try {
        $finalResponse = Invoke-WebRequest -Uri "$ApiBaseUrl/api/properties?PageNumber=1&PageSize=20" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        $finalJson = $finalResponse.Content | ConvertFrom-Json
        $finalCount = $finalJson.TotalCount
        
        Write-Host "Final property count: $finalCount" -ForegroundColor Cyan
        
        if ($finalCount -ge $PropertyCount) {
            Write-Host "✅ SUCCESS: Target reached ($finalCount properties)" -ForegroundColor Green
        }
        elseif ($finalCount > 0) {
            Write-Host "⚠️  Partial success: $finalCount properties seeded (target: $PropertyCount)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "⚠️  Could not verify final count: $_" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Seeding Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Test property search with:" -ForegroundColor Yellow
Write-Host '  curl "http://localhost:5111/api/properties?PageNumber=1&PageSize=20"' -ForegroundColor White
Write-Host ""
