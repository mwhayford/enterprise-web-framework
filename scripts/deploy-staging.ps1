# Deploy Staging Environment
# Usage: .\scripts\deploy-staging.ps1

$ErrorActionPreference = "Stop"

Write-Host "🚀 Deploying Staging Infrastructure..." -ForegroundColor Cyan
Write-Host ""

# Navigate to Terraform directory
Set-Location "$PSScriptRoot\..\infrastructure\terraform\environments\staging"

# Initialize if needed
if (!(Test-Path ".terraform")) {
    Write-Host "📦 Initializing Terraform..." -ForegroundColor Yellow
    terraform init
}

# Plan deployment
Write-Host "📋 Planning deployment..." -ForegroundColor Yellow
terraform plan -out=tfplan

Write-Host ""
Write-Host "⚠️  Review the plan above" -ForegroundColor Yellow
$confirm = Read-Host "Do you want to proceed with deployment? (yes/no)"

if ($confirm -ne "yes") {
    Write-Host "❌ Deployment cancelled" -ForegroundColor Red
    exit 0
}

# Apply infrastructure
Write-Host ""
Write-Host "🏗️  Creating infrastructure (this takes 20-25 minutes)..." -ForegroundColor Cyan
$startTime = Get-Date
terraform apply tfplan

$duration = (Get-Date) - $startTime
Write-Host ""
Write-Host "✅ Infrastructure deployed in $($duration.TotalMinutes.ToString('0.0')) minutes!" -ForegroundColor Green

# Save outputs
Write-Host "📝 Saving outputs..." -ForegroundColor Yellow
terraform output | Out-File -FilePath "..\..\outputs.txt"

# Get cluster name
$clusterName = terraform output -raw eks_cluster_name

# Configure kubectl
Write-Host "🔧 Configuring kubectl..." -ForegroundColor Yellow
aws eks update-kubeconfig --region us-east-1 --name $clusterName

# Test kubectl connection
Write-Host "🔍 Verifying cluster connection..." -ForegroundColor Yellow
kubectl get nodes

Write-Host ""
Write-Host "✨ Deployment Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Build and push Docker images to ECR"
Write-Host "  2. Deploy application to Kubernetes"
Write-Host "  3. Run database migrations"
Write-Host "  4. Test the application"
Write-Host ""
Write-Host "📖 See docs/staging-test-workflow.md for detailed instructions"
Write-Host ""
Write-Host "💰 Estimated cost: ~`$0.25/hour while running"
Write-Host ""

# Return to original directory
Set-Location $PSScriptRoot\..

