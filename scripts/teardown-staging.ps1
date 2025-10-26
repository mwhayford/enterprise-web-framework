# Teardown Staging Environment
# Usage: .\scripts\teardown-staging.ps1

$ErrorActionPreference = "Stop"

Write-Host "🗑️  Tearing Down Staging Environment..." -ForegroundColor Red
Write-Host ""

Write-Host "⚠️  WARNING: This will permanently delete:" -ForegroundColor Yellow
Write-Host "  - EKS cluster and all nodes"
Write-Host "  - RDS database (all data will be lost)"
Write-Host "  - ElastiCache Redis cluster"
Write-Host "  - All networking resources"
Write-Host "  - ECR repositories and images"
Write-Host ""

$confirm = Read-Host "Are you sure you want to proceed? Type 'destroy' to confirm"

if ($confirm -ne "destroy") {
    Write-Host "❌ Teardown cancelled" -ForegroundColor Green
    exit 0
}

# Optional: Create snapshots before deletion
Write-Host ""
$snapshot = Read-Host "Create database snapshot before deletion? (yes/no)"

if ($snapshot -eq "yes") {
    Write-Host "📸 Creating RDS snapshot..." -ForegroundColor Yellow
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    aws rds create-db-snapshot `
        --db-instance-identifier rentalmanager-staging-db `
        --db-snapshot-identifier "staging-final-$timestamp"
    
    Write-Host "✅ Snapshot created: staging-final-$timestamp" -ForegroundColor Green
}

# Step 1: Delete Kubernetes resources
Write-Host ""
Write-Host "1️⃣  Deleting Kubernetes resources..." -ForegroundColor Yellow

try {
    kubectl delete namespace rentalmanager-staging --ignore-not-found=true --timeout=5m
    Write-Host "✅ Kubernetes namespace deleted" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Warning: Could not delete Kubernetes namespace (may not exist)" -ForegroundColor Yellow
}

# Wait a moment for load balancers to be deleted
Write-Host "⏳ Waiting for load balancers to be cleaned up (30 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Step 2: Destroy Terraform infrastructure
Write-Host ""
Write-Host "2️⃣  Destroying Terraform infrastructure..." -ForegroundColor Yellow

Set-Location "$PSScriptRoot\..\infrastructure\terraform\environments\staging"

# Show what will be destroyed
Write-Host "📋 Planning destruction..." -ForegroundColor Yellow
terraform plan -destroy

Write-Host ""
$finalConfirm = Read-Host "Proceed with destruction? (yes/no)"

if ($finalConfirm -ne "yes") {
    Write-Host "❌ Teardown cancelled" -ForegroundColor Red
    Set-Location $PSScriptRoot\..
    exit 0
}

# Execute destroy
$startTime = Get-Date
terraform destroy -auto-approve

$duration = (Get-Date) - $startTime

Write-Host ""
Write-Host "✅ Infrastructure destroyed in $($duration.TotalMinutes.ToString('0.0')) minutes!" -ForegroundColor Green

# Step 3: Verify deletion
Write-Host ""
Write-Host "3️⃣  Verifying deletion..." -ForegroundColor Yellow

$eksCount = (aws eks list-clusters --region us-east-1 --query 'clusters' --output json | ConvertFrom-Json).Count
$rdsCount = (aws rds describe-db-instances --region us-east-1 --query 'DBInstances[?contains(DBInstanceIdentifier, `rentalmanager-staging`)]' --output json | ConvertFrom-Json).Count

if ($eksCount -eq 0 -and $rdsCount -eq 0) {
    Write-Host "✅ All resources successfully deleted" -ForegroundColor Green
} else {
    Write-Host "⚠️  Some resources may still exist:" -ForegroundColor Yellow
    Write-Host "   EKS clusters: $eksCount"
    Write-Host "   RDS instances: $rdsCount"
    Write-Host "   Check AWS Console for manual cleanup if needed"
}

# Step 4: Clean up local state
Write-Host ""
Write-Host "4️⃣  Cleaning up local configuration..." -ForegroundColor Yellow

try {
    kubectl config delete-context "arn:aws:eks:us-east-1:*:cluster/rentalmanager-staging-eks" 2>$null
    Write-Host "✅ kubectl context removed" -ForegroundColor Green
} catch {
    Write-Host "⚠️  kubectl context not found (already removed)" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "✨ Teardown Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "💰 Cost Status:" -ForegroundColor Cyan
Write-Host "   Current: ~`$2/month (state storage only)"
Write-Host "   Savings: ~`$176/month (vs running 24/7)"
Write-Host ""
Write-Host "📦 What Remains:" -ForegroundColor Cyan
Write-Host "   ✅ S3 bucket (terraform state)"
Write-Host "   ✅ DynamoDB table (state locks)"
Write-Host "   ✅ Snapshots (if created)"
Write-Host ""
Write-Host "🔄 To Redeploy:" -ForegroundColor Cyan
Write-Host "   Run: .\scripts\deploy-staging.ps1"
Write-Host "   Time: ~25 minutes"
Write-Host ""

# Return to original directory
Set-Location $PSScriptRoot\..

