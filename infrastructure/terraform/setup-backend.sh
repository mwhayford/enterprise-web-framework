#!/bin/bash
# Copyright (c) RentalManager. All Rights Reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

# Script to setup S3 backend for Terraform state management
set -e

# Configuration
AWS_REGION="${AWS_REGION:-us-east-1}"
S3_BUCKET_NAME="rentalmanager-terraform-state"
DYNAMODB_TABLE_NAME="rentalmanager-terraform-locks"

echo "======================================"
echo "Terraform Backend Setup"
echo "======================================"
echo "Region: $AWS_REGION"
echo "S3 Bucket: $S3_BUCKET_NAME"
echo "DynamoDB Table: $DYNAMODB_TABLE_NAME"
echo "======================================"
echo ""

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
    echo "ERROR: AWS CLI is not installed. Please install it first."
    echo "Visit: https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html"
    exit 1
fi

# Check AWS credentials
echo "Checking AWS credentials..."
if ! aws sts get-caller-identity &> /dev/null; then
    echo "ERROR: AWS credentials not configured or invalid."
    echo "Run: aws configure"
    exit 1
fi

AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo "✓ AWS Account: $AWS_ACCOUNT_ID"
echo ""

# Create S3 bucket for Terraform state
echo "Creating S3 bucket for Terraform state..."
if aws s3 ls "s3://$S3_BUCKET_NAME" 2>&1 | grep -q 'NoSuchBucket'; then
    aws s3api create-bucket \
        --bucket "$S3_BUCKET_NAME" \
        --region "$AWS_REGION" \
        $(if [ "$AWS_REGION" != "us-east-1" ]; then echo "--create-bucket-configuration LocationConstraint=$AWS_REGION"; fi)
    
    echo "✓ S3 bucket created: $S3_BUCKET_NAME"
else
    echo "✓ S3 bucket already exists: $S3_BUCKET_NAME"
fi

# Enable versioning
echo "Enabling versioning on S3 bucket..."
aws s3api put-bucket-versioning \
    --bucket "$S3_BUCKET_NAME" \
    --versioning-configuration Status=Enabled \
    --region "$AWS_REGION"
echo "✓ Versioning enabled"

# Enable encryption
echo "Enabling server-side encryption..."
aws s3api put-bucket-encryption \
    --bucket "$S3_BUCKET_NAME" \
    --server-side-encryption-configuration '{
        "Rules": [{
            "ApplyServerSideEncryptionByDefault": {
                "SSEAlgorithm": "AES256"
            },
            "BucketKeyEnabled": true
        }]
    }' \
    --region "$AWS_REGION"
echo "✓ Encryption enabled"

# Block public access
echo "Blocking public access..."
aws s3api put-public-access-block \
    --bucket "$S3_BUCKET_NAME" \
    --public-access-block-configuration \
        "BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true" \
    --region "$AWS_REGION"
echo "✓ Public access blocked"

# Enable lifecycle policy to clean up old versions
echo "Configuring lifecycle policy..."
aws s3api put-bucket-lifecycle-configuration \
    --bucket "$S3_BUCKET_NAME" \
    --lifecycle-configuration '{
        "Rules": [{
            "Id": "DeleteOldVersions",
            "Status": "Enabled",
            "NoncurrentVersionExpiration": {
                "NoncurrentDays": 90
            }
        }]
    }' \
    --region "$AWS_REGION"
echo "✓ Lifecycle policy configured"

# Create DynamoDB table for state locking
echo ""
echo "Creating DynamoDB table for state locking..."
if aws dynamodb describe-table --table-name "$DYNAMODB_TABLE_NAME" --region "$AWS_REGION" &> /dev/null; then
    echo "✓ DynamoDB table already exists: $DYNAMODB_TABLE_NAME"
else
    aws dynamodb create-table \
        --table-name "$DYNAMODB_TABLE_NAME" \
        --attribute-definitions AttributeName=LockID,AttributeType=S \
        --key-schema AttributeName=LockID,KeyType=HASH \
        --billing-mode PAY_PER_REQUEST \
        --region "$AWS_REGION" \
        --tags Key=Environment,Value=shared Key=ManagedBy,Value=Terraform
    
    echo "✓ DynamoDB table created: $DYNAMODB_TABLE_NAME"
    
    # Wait for table to be active
    echo "Waiting for table to be active..."
    aws dynamodb wait table-exists \
        --table-name "$DYNAMODB_TABLE_NAME" \
        --region "$AWS_REGION"
    echo "✓ Table is active"
fi

echo ""
echo "======================================"
echo "✓ Terraform Backend Setup Complete!"
echo "======================================"
echo ""
echo "Backend Configuration:"
echo "  bucket         = \"$S3_BUCKET_NAME\""
echo "  region         = \"$AWS_REGION\""
echo "  dynamodb_table = \"$DYNAMODB_TABLE_NAME\""
echo "  encrypt        = true"
echo ""
echo "Next steps:"
echo "  1. Navigate to an environment directory:"
echo "     cd infrastructure/terraform/environments/staging"
echo "  2. Initialize Terraform:"
echo "     terraform init"
echo "  3. Plan your infrastructure:"
echo "     terraform plan"
echo "  4. Apply your infrastructure:"
echo "     terraform apply"
echo ""

