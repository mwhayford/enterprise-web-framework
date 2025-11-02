# Setup script for managing secrets securely
# This script helps set up secrets using .NET User Secrets or PowerShell environment variables

param(
    [switch] $UseUserSecrets,
    [switch] $UseEnvVars,
    [switch] $ShowHelp
)

$ErrorActionPreference = "Stop"

if ($ShowHelp) {
    Write-Host "=== Secrets Management Setup ===" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "This script helps you manage secrets securely:" -ForegroundColor Yellow
    Write-Host "  - .NET User Secrets (for local development)" -ForegroundColor Green
    Write-Host "  - PowerShell Environment Variables (for Docker deployment)" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Cyan
    Write-Host "  .\scripts\setup-secrets.ps1 -UseUserSecrets    # Set up .NET User Secrets" -ForegroundColor White
    Write-Host "  .\scripts\setup-secrets.ps1 -UseEnvVars        # Set up PowerShell env vars for Docker" -ForegroundColor White
    Write-Host ""
    Write-Host "Secrets to configure:" -ForegroundColor Yellow
    Write-Host "  - Authentication:Google:ClientId" -ForegroundColor Gray
    Write-Host "  - Authentication:Google:ClientSecret" -ForegroundColor Gray
    Write-Host "  - JwtSettings:SecretKey" -ForegroundColor Gray
    Write-Host "  - StripeSettings:SecretKey" -ForegroundColor Gray
    Write-Host "  - StripeSettings:WebhookSecret" -ForegroundColor Gray
    exit 0
}

$apiProject = "src\backend\RentalManager.API\RentalManager.API.csproj"

if (-not (Test-Path $apiProject)) {
    Write-Host "❌ ERROR: Cannot find API project at $apiProject" -ForegroundColor Red
    exit 1
}

Write-Host "=== Secrets Management Setup ===" -ForegroundColor Cyan
Write-Host ""

if ($UseUserSecrets) {
    Write-Host "Setting up .NET User Secrets for local development..." -ForegroundColor Yellow
    Write-Host ""
    
    # Initialize User Secrets if not already done
    Write-Host "Initializing User Secrets..." -ForegroundColor Green
    dotnet user-secrets init --project $apiProject
    
    Write-Host ""
    Write-Host "Please enter your secrets (or press Enter to skip):" -ForegroundColor Cyan
    Write-Host ""
    
    $clientId = Read-Host "Google OAuth Client ID"
    if ($clientId) {
        dotnet user-secrets set "Authentication:Google:ClientId" $clientId --project $apiProject
        Write-Host "✅ Set Authentication:Google:ClientId" -ForegroundColor Green
    }
    
    $clientSecret = Read-Host "Google OAuth Client Secret" -AsSecureString
    if ($clientSecret) {
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($clientSecret)
        $plainSecret = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
        dotnet user-secrets set "Authentication:Google:ClientSecret" $plainSecret --project $apiProject
        Write-Host "✅ Set Authentication:Google:ClientSecret" -ForegroundColor Green
    }
    
    $jwtKey = Read-Host "JWT Secret Key (min 32 chars)" -AsSecureString
    if ($jwtKey) {
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($jwtKey)
        $plainKey = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
        dotnet user-secrets set "JwtSettings:SecretKey" $plainKey --project $apiProject
        Write-Host "✅ Set JwtSettings:SecretKey" -ForegroundColor Green
    }
    
    $stripeSecret = Read-Host "Stripe Secret Key" -AsSecureString
    if ($stripeSecret) {
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($stripeSecret)
        $plainSecret = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
        dotnet user-secrets set "StripeSettings:SecretKey" $plainSecret --project $apiProject
        Write-Host "✅ Set StripeSettings:SecretKey" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "✅ User Secrets configured!" -ForegroundColor Green
    Write-Host "   These are stored locally and NOT in source control." -ForegroundColor Gray
    Write-Host ""
    Write-Host "To view secrets: dotnet user-secrets list --project $apiProject" -ForegroundColor Cyan
    Write-Host "To remove: dotnet user-secrets remove \"Key\" --project $apiProject" -ForegroundColor Cyan
}

if ($UseEnvVars) {
    Write-Host "Setting up PowerShell environment variables for Docker..." -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "Please enter your secrets (or press Enter to skip):" -ForegroundColor Cyan
    Write-Host ""
    
    $clientId = Read-Host "Google OAuth Client ID"
    if ($clientId) {
        $env:Authentication__Google__ClientId = $clientId
        Write-Host "✅ Set Authentication__Google__ClientId (session only)" -ForegroundColor Green
        Write-Host "   To persist, add to your PowerShell profile or use set-docker-env.ps1" -ForegroundColor Gray
    }
    
    $clientSecret = Read-Host "Google OAuth Client Secret" -AsSecureString
    if ($clientSecret) {
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($clientSecret)
        $plainSecret = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
        $env:Authentication__Google__ClientSecret = $plainSecret
        Write-Host "✅ Set Authentication__Google__ClientSecret (session only)" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "✅ Environment variables set for current session" -ForegroundColor Green
    Write-Host "   Use .\scripts\set-docker-env.ps1 to set from appsettings.Development.json" -ForegroundColor Cyan
    Write-Host "   Or use .\scripts\deploy-local.ps1 for full deployment" -ForegroundColor Cyan
}

if (-not $UseUserSecrets -and -not $UseEnvVars) {
    Write-Host "Please specify -UseUserSecrets or -UseEnvVars" -ForegroundColor Yellow
    Write-Host "Run with -ShowHelp for usage information" -ForegroundColor Cyan
    exit 1
}

