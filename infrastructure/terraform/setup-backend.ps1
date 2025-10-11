# Copyright (c) Core. All Rights Reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

# Script to setup S3 backend for Terraform state management
# Usage: .\setup-backend.ps1

param(
    [string]$AwsRegion = "us-east-1",
    [string]$S3BucketName = "core-terraform-state",
    [string]$DynamoDbTableName = "core-terraform-locks"
)

$ErrorActionPreference = "Stop"

Write-Host "======================================"
Write-Host "Terraform Backend Setup"
Write-Host "======================================"
Write-Host "Region: $AwsRegion"
Write-Host "S3 Bucket: $S3BucketName"
Write-Host "DynamoDB Table: $DynamoDbTableName"
Write-Host "======================================"
Write-Host ""

# Check if AWS CLI is installed
try {
    $null = Get-Command aws -ErrorAction Stop
} catch {
    Write-Host "ERROR: AWS CLI is not installed. Please install it first." -ForegroundColor Red
    Write-Host "Visit: https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html"
    exit 1
}

# Check AWS credentials
Write-Host "Checking AWS credentials..."
try {
    $accountId = aws sts get-caller-identity --query Account --output text
    Write-Host "✓ AWS Account: $accountId" -ForegroundColor Green
} catch {
    Write-Host "ERROR: AWS credentials not configured or invalid." -ForegroundColor Red
    Write-Host "Run: aws configure"
    exit 1
}
Write-Host ""

# Create S3 bucket for Terraform state
Write-Host "Creating S3 bucket for Terraform state..."
try {
    aws s3 ls "s3://$S3BucketName" 2>&1 | Out-Null
    Write-Host "✓ S3 bucket already exists: $S3BucketName" -ForegroundColor Green
} catch {
    if ($AwsRegion -eq "us-east-1") {
        aws s3api create-bucket --bucket $S3BucketName --region $AwsRegion
    } else {
        aws s3api create-bucket --bucket $S3BucketName --region $AwsRegion --create-bucket-configuration "LocationConstraint=$AwsRegion"
    }
    Write-Host "✓ S3 bucket created: $S3BucketName" -ForegroundColor Green
}

# Enable versioning
Write-Host "Enabling versioning on S3 bucket..."
aws s3api put-bucket-versioning `
    --bucket $S3BucketName `
    --versioning-configuration Status=Enabled `
    --region $AwsRegion
Write-Host "✓ Versioning enabled" -ForegroundColor Green

# Enable encryption
Write-Host "Enabling server-side encryption..."
$encryptionConfig = @{
    Rules = @(
        @{
            ApplyServerSideEncryptionByDefault = @{
                SSEAlgorithm = "AES256"
            }
            BucketKeyEnabled = $true
        }
    )
} | ConvertTo-Json -Depth 10 -Compress

aws s3api put-bucket-encryption `
    --bucket $S3BucketName `
    --server-side-encryption-configuration $encryptionConfig `
    --region $AwsRegion
Write-Host "✓ Encryption enabled" -ForegroundColor Green

# Block public access
Write-Host "Blocking public access..."
aws s3api put-public-access-block `
    --bucket $S3BucketName `
    --public-access-block-configuration "BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true" `
    --region $AwsRegion
Write-Host "✓ Public access blocked" -ForegroundColor Green

# Enable lifecycle policy to clean up old versions
Write-Host "Configuring lifecycle policy..."
$lifecycleConfig = @{
    Rules = @(
        @{
            Id = "DeleteOldVersions"
            Status = "Enabled"
            NoncurrentVersionExpiration = @{
                NoncurrentDays = 90
            }
        }
    )
} | ConvertTo-Json -Depth 10 -Compress

aws s3api put-bucket-lifecycle-configuration `
    --bucket $S3BucketName `
    --lifecycle-configuration $lifecycleConfig `
    --region $AwsRegion
Write-Host "✓ Lifecycle policy configured" -ForegroundColor Green

# Create DynamoDB table for state locking
Write-Host ""
Write-Host "Creating DynamoDB table for state locking..."
try {
    aws dynamodb describe-table --table-name $DynamoDbTableName --region $AwsRegion 2>&1 | Out-Null
    Write-Host "✓ DynamoDB table already exists: $DynamoDbTableName" -ForegroundColor Green
} catch {
    aws dynamodb create-table `
        --table-name $DynamoDbTableName `
        --attribute-definitions AttributeName=LockID,AttributeType=S `
        --key-schema AttributeName=LockID,KeyType=HASH `
        --billing-mode PAY_PER_REQUEST `
        --region $AwsRegion `
        --tags Key=Environment,Value=shared Key=ManagedBy,Value=Terraform
    
    Write-Host "✓ DynamoDB table created: $DynamoDbTableName" -ForegroundColor Green
    
    # Wait for table to be active
    Write-Host "Waiting for table to be active..."
    aws dynamodb wait table-exists --table-name $DynamoDbTableName --region $AwsRegion
    Write-Host "✓ Table is active" -ForegroundColor Green
}

Write-Host ""
Write-Host "======================================"
Write-Host "✓ Terraform Backend Setup Complete!" -ForegroundColor Green
Write-Host "======================================"
Write-Host ""
Write-Host "Backend Configuration:"
Write-Host "  bucket         = `"$S3BucketName`""
Write-Host "  region         = `"$AwsRegion`""
Write-Host "  dynamodb_table = `"$DynamoDbTableName`""
Write-Host "  encrypt        = true"
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Navigate to an environment directory:"
Write-Host "     cd infrastructure\terraform\environments\staging"
Write-Host "  2. Initialize Terraform:"
Write-Host "     terraform init"
Write-Host "  3. Plan your infrastructure:"
Write-Host "     terraform plan"
Write-Host "  4. Apply your infrastructure:"
Write-Host "     terraform apply"
Write-Host ""

