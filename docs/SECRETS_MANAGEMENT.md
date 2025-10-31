# Secrets Management Guide

This document describes how to manage secrets securely in the RentalManager project without committing them to source control.

## Principles

1. **NEVER commit secrets to source control**
2. **Use .NET User Secrets for local development**
3. **Use environment variables for Docker deployments**
4. **Use placeholder values in `appsettings.Development.json`**

## What Secrets Need to be Configured?

- `Authentication:Google:ClientId` - Google OAuth Client ID
- `Authentication:Google:ClientSecret` - Google OAuth Client Secret
- `JwtSettings:SecretKey` - JWT signing key (minimum 32 characters)
- `StripeSettings:SecretKey` - Stripe API secret key
- `StripeSettings:WebhookSecret` - Stripe webhook signing secret

## Local Development (Visual Studio / dotnet run)

### Option 1: .NET User Secrets (Recommended)

.NET User Secrets are stored locally and never committed to source control.

#### Setup

```powershell
# Initialize User Secrets (only needed once)
cd src/backend/RentalManager.API
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id" 
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-secret"
dotnet user-secrets set "JwtSettings:SecretKey" "your-jwt-key"
dotnet user-secrets set "StripeSettings:SecretKey" "your-stripe-secret"
```

#### View Secrets

```powershell
dotnet user-secrets list
```

#### Remove Secrets

```powershell
dotnet user-secrets remove "Authentication:Google:ClientId"
```

#### Location

User Secrets are stored at:
- **Windows**: `%APPDATA%\Microsoft\UserSecrets\RentalManager.API-Development\secrets.json`
- **Linux/Mac**: `~/.microsoft/usersecrets/RentalManager.API-Development/secrets.json`

### Option 2: PowerShell Script

```powershell
.\scripts\setup-secrets.ps1 -UseUserSecrets
```

## Docker Deployment

For Docker deployments, secrets must be provided via environment variables.

### Method 1: Using PowerShell Environment Variables

```powershell
# Set environment variables (session-scoped)
$env:Authentication__Google__ClientId = "your-client-id"
$env:Authentication__Google__ClientSecret = "your-secret"

# Deploy
docker-compose up -d --build backend
```

**Note**: Double underscores (`__`) are used to represent nested JSON keys (`:`).

### Method 2: Using Helper Script

The `set-docker-env.ps1` script can read from User Secrets or prompt for values:

```powershell
.\scripts\set-docker-env.ps1
docker-compose up -d --build backend
```

### Method 3: Full Deployment Script

```powershell
.\scripts\deploy-local.ps1
```

This script:
1. Reads from User Secrets or prompts for values
2. Sets PowerShell environment variables
3. Deploys to Docker

## File Structure

```
src/backend/RentalManager.API/
├── appsettings.json                    # Base config (no secrets, committed)
├── appsettings.Development.json        # Local overrides (placeholders only, NOT committed)
└── appsettings.Development.json.template  # Template (committed)
```

## Configuration Precedence

ASP.NET Core reads configuration in this order (later sources override earlier):

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific)
3. **User Secrets** (only in Development)
4. **Environment Variables** (highest priority)
5. Command-line arguments

When running in Docker (`DOTNET_RUNNING_IN_CONTAINER=true`):
- `appsettings.Development.json` and `appsettings.Production.json` are **removed**
- Only environment variables are used for secrets

## Security Checklist

- [ ] `appsettings.Development.json` contains only placeholders
- [ ] `.gitignore` excludes `appsettings.Development.json`
- [ ] Secrets are stored in User Secrets (local) or environment variables (Docker)
- [ ] No secrets in source control history
- [ ] Docker builds exclude secrets from images (`.dockerignore`)

## Getting Secrets

### Google OAuth

1. Go to [Google Cloud Console](https://console.cloud.google.com/apis/credentials)
2. Select your project
3. Create or select OAuth 2.0 Client ID
4. Copy Client ID and Client Secret
5. Add redirect URI: `http://localhost:5111/signin-google`

### JWT Secret Key

Generate a secure random key (minimum 32 characters):

```powershell
# PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

### Stripe Keys

1. Go to [Stripe Dashboard](https://dashboard.stripe.com/test/apikeys)
2. Copy API keys and webhook secrets

## Troubleshooting

### "Secrets not found" error

**Local Development:**
- Ensure User Secrets are initialized: `dotnet user-secrets init`
- Verify secrets exist: `dotnet user-secrets list`

**Docker:**
- Check environment variables: `docker exec rentalmanager-backend printenv | Select-String "Authentication"`
- Verify `docker-compose.override.yml` uses `${Variable}` syntax

### "invalid_client" error (Google OAuth)

- ClientSecret may have been rotated in Google Cloud Console
- Update User Secrets or environment variables with new secret
- Verify redirect URI matches exactly: `http://localhost:5111/signin-google`

## Best Practices

1. **Rotate secrets regularly** (especially if exposed)
2. **Use different secrets for dev/staging/production**
3. **Never share secrets via email, chat, or screenshots**
4. **Use secret management tools in production** (Azure Key Vault, AWS Secrets Manager, etc.)
5. **Audit secrets access** in production environments

## References

- [.NET User Secrets Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Docker Environment Variables](https://docs.docker.com/compose/environment-variables/)

