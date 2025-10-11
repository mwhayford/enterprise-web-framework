# Database management script for Core application
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("start", "stop", "restart", "status", "logs", "migrate", "seed")]
    [string]$Action = "start"
)

$ErrorActionPreference = "Stop"

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Start-Database {
    Write-Info "Starting database services..."
    docker-compose -f docker-compose.dev.yml up -d postgres redis
    
    Write-Info "Waiting for services to be healthy..."
    $maxAttempts = 30
    $attempt = 0
    
    do {
        $attempt++
        Start-Sleep -Seconds 2
        
        $postgresHealth = docker inspect core-postgres-dev --format='{{.State.Health.Status}}' 2>$null
        $redisHealth = docker inspect core-redis-dev --format='{{.State.Health.Status}}' 2>$null
        
        if ($postgresHealth -eq "healthy" -and $redisHealth -eq "healthy") {
            Write-Success "Database services are healthy and ready!"
            return
        }
        
        Write-Info "Waiting for services... (attempt $attempt/$maxAttempts)"
    } while ($attempt -lt $maxAttempts)
    
    Write-Error "Services failed to become healthy within timeout"
    exit 1
}

function Stop-Database {
    Write-Info "Stopping database services..."
    docker-compose -f docker-compose.dev.yml down
    Write-Success "Database services stopped"
}

function Restart-Database {
    Write-Info "Restarting database services..."
    Stop-Database
    Start-Sleep -Seconds 2
    Start-Database
}

function Show-Status {
    Write-Info "Database services status:"
    docker-compose -f docker-compose.dev.yml ps
}

function Show-Logs {
    Write-Info "Showing database logs (press Ctrl+C to exit):"
    docker-compose -f docker-compose.dev.yml logs -f postgres redis
}

function Run-Migration {
    Write-Info "Running database migrations..."
    
    # Check if services are running
    $postgresStatus = docker inspect core-postgres-dev --format='{{.State.Status}}' 2>$null
    if ($postgresStatus -ne "running") {
        Write-Error "PostgreSQL service is not running. Please start it first with: .\scripts\database.ps1 start"
        exit 1
    }
    
    # Run migrations
    Set-Location "src\backend\Core.API"
    try {
        dotnet ef database update --project ..\Core.Infrastructure
        Write-Success "Database migrations completed successfully"
    }
    catch {
        Write-Error "Migration failed: $_"
        exit 1
    }
    finally {
        Set-Location "..\..\.."
    }
}

function Seed-Database {
    Write-Info "Seeding database with initial data..."
    
    # Check if services are running
    $postgresStatus = docker inspect core-postgres-dev --format='{{.State.Status}}' 2>$null
    if ($postgresStatus -ne "running") {
        Write-Error "PostgreSQL service is not running. Please start it first with: .\scripts\database.ps1 start"
        exit 1
    }
    
    # TODO: Implement database seeding
    Write-Info "Database seeding not yet implemented"
    Write-Success "Database seeding completed (placeholder)"
}

# Main script logic
switch ($Action.ToLower()) {
    "start" { Start-Database }
    "stop" { Stop-Database }
    "restart" { Restart-Database }
    "status" { Show-Status }
    "logs" { Show-Logs }
    "migrate" { Run-Migration }
    "seed" { Seed-Database }
    default {
        Write-Error "Invalid action: $Action"
        Write-Info "Valid actions: start, stop, restart, status, logs, migrate, seed"
        exit 1
    }
}

