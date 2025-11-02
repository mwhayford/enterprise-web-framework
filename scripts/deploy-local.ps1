# Deploy RentalManager to local Docker
# Automatically sets environment variables from appsettings.Development.json

$ErrorActionPreference = "Stop"

Write-Host "=== RentalManager Local Deployment ===" -ForegroundColor Cyan
Write-Host ""

# Get the root directory (parent of scripts/)
$rootDir = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Set-Location $rootDir

# Step 1: Set environment variables
Write-Host "Step 1: Setting environment variables for Docker..." -ForegroundColor Yellow
& (Join-Path $PSScriptRoot "set-docker-env.ps1")

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to set environment variables" -ForegroundColor Red
    Write-Host "   Run: .\scripts\set-docker-env.ps1" -ForegroundColor Cyan
    exit 1
}

# Verify environment variables are set
if (-not $env:Authentication__Google__ClientId -or -not $env:Authentication__Google__ClientSecret) {
    Write-Host "⚠️  WARNING: Google auth environment variables not set" -ForegroundColor Yellow
    Write-Host "   They will default to blank strings in docker-compose" -ForegroundColor Yellow
    Write-Host "   Google authentication will not work unless set" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Step 2: Stopping existing containers..." -ForegroundColor Yellow
docker-compose down

Write-Host ""
Write-Host "Step 3: Building and starting containers..." -ForegroundColor Yellow
docker-compose up -d --build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker compose failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 4: Waiting for services to be healthy..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host ""
Write-Host "Step 5: Verifying environment variables in container..." -ForegroundColor Yellow
$envCheck = docker exec rentalmanager-backend printenv | Select-String "Authentication__Google"
if ($envCheck) {
    Write-Host "✅ Environment variables verified in container" -ForegroundColor Green
} else {
    Write-Host "⚠️  WARNING: Environment variables not found in container" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Deployment Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "Services:" -ForegroundColor Cyan
Write-Host "  Backend API:    http://localhost:5111" -ForegroundColor White
Write-Host "  Frontend:       http://localhost:5173" -ForegroundColor White
Write-Host "  PostgreSQL:     localhost:5433" -ForegroundColor White
Write-Host ""
Write-Host "View logs:" -ForegroundColor Cyan
Write-Host "  docker-compose logs -f backend" -ForegroundColor White

