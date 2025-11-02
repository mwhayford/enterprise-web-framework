# Sets PowerShell environment variables for Docker deployment
# Reads from .NET User Secrets or prompts for values
# NOTE: appsettings.Development.json should NOT contain real secrets!

param(
    [switch] $Prompt
)

$ErrorActionPreference = "Stop"

Write-Host "=== Setting Docker Environment Variables ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script reads secrets from .NET User Secrets for Docker deployment." -ForegroundColor Yellow
Write-Host ""

$apiProject = Join-Path $PSScriptRoot "..\src\backend\RentalManager.API\RentalManager.API.csproj"

if (-not (Test-Path $apiProject)) {
    Write-Host "❌ ERROR: Cannot find API project at $apiProject" -ForegroundColor Red
    exit 1
}

# Try to read from User Secrets first
$clientId = $null
$clientSecret = $null

try {
    $secretsOutput = dotnet user-secrets list --project $apiProject 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        $secrets = @{}
        $secretsOutput | Where-Object { $_ -match "(\S+)\s*=\s*(.+)" } | ForEach-Object {
            if ($_ -match "(\S+)\s*=\s*(.+)") {
                $secrets[$matches[1]] = $matches[2]
            }
        }
        
        if ($secrets.ContainsKey("Authentication:Google:ClientId")) {
            $clientId = $secrets["Authentication:Google:ClientId"]
        }
        if ($secrets.ContainsKey("Authentication:Google:ClientSecret")) {
            $clientSecret = $secrets["Authentication:Google:ClientSecret"]
        }
    }
} catch {
    Write-Host "⚠️  Could not read User Secrets: $_" -ForegroundColor Yellow
    Write-Host "   Run: dotnet user-secrets init --project $apiProject" -ForegroundColor Cyan
}

# If not found in User Secrets, prompt for values
if ([string]::IsNullOrWhiteSpace($clientId) -or [string]::IsNullOrWhiteSpace($clientSecret) -or $Prompt) {
    Write-Host "Secrets not found in User Secrets or -Prompt specified." -ForegroundColor Yellow
    Write-Host "Please enter your secrets:" -ForegroundColor Cyan
    Write-Host ""
    
    if ([string]::IsNullOrWhiteSpace($clientId)) {
        $clientId = Read-Host "Google OAuth Client ID"
    } else {
        Write-Host "Using ClientId from User Secrets" -ForegroundColor Green
    }
    
    if ([string]::IsNullOrWhiteSpace($clientSecret)) {
        $secureSecret = Read-Host "Google OAuth Client Secret" -AsSecureString
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureSecret)
        $clientSecret = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    } else {
        Write-Host "Using ClientSecret from User Secrets" -ForegroundColor Green
    }
}

if ([string]::IsNullOrWhiteSpace($clientId) -or [string]::IsNullOrWhiteSpace($clientSecret)) {
    Write-Host "❌ ERROR: ClientId and ClientSecret are required" -ForegroundColor Red
    Write-Host ""
    Write-Host "Set them using User Secrets:" -ForegroundColor Yellow
    Write-Host "  dotnet user-secrets set \"Authentication:Google:ClientId\" \"your-id\" --project $apiProject" -ForegroundColor Cyan
    Write-Host "  dotnet user-secrets set \"Authentication:Google:ClientSecret\" \"your-secret\" --project $apiProject" -ForegroundColor Cyan
    exit 1
}

# Set environment variables (session-scoped)
$env:Authentication__Google__ClientId = $clientId
$env:Authentication__Google__ClientSecret = $clientSecret

Write-Host ""
Write-Host "✅ Environment variables set:" -ForegroundColor Green
Write-Host "   Authentication__Google__ClientId = $clientId" -ForegroundColor Gray
Write-Host "   Authentication__Google__ClientSecret = $($clientSecret.Substring(0, [Math]::Min(20, $clientSecret.Length)))..." -ForegroundColor Gray
Write-Host ""
Write-Host "✅ Ready for: docker-compose up -d backend" -ForegroundColor Green
Write-Host ""
Write-Host "NOTE: These are session-scoped. For persistent storage, use User Secrets." -ForegroundColor Yellow
