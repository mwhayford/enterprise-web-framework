# GitHub Secrets Configuration

This document outlines all GitHub secrets required for the CI/CD pipeline to deploy to AWS.

## Overview

GitHub secrets are used to securely store sensitive information like AWS credentials, API keys, and other configuration values needed for automated deployment.

## Required Secrets

### AWS Credentials

These secrets are required for deploying to AWS infrastructure.

#### `AWS_ACCESS_KEY_ID`
- **Description**: AWS Access Key ID for the deployment IAM user
- **Required**: Yes
- **Environment**: All (staging, production)
- **How to obtain**:
  ```bash
  # Create IAM user with programmatic access
  aws iam create-user --user-name github-actions-deployer
  
  # Attach required policies (see IAM Policies section below)
  aws iam attach-user-policy --user-name github-actions-deployer \
    --policy-arn arn:aws:iam::aws:policy/PowerUserAccess
  
  # Create access key
  aws iam create-access-key --user-name github-actions-deployer
  ```

#### `AWS_SECRET_ACCESS_KEY`
- **Description**: AWS Secret Access Key for the deployment IAM user
- **Required**: Yes
- **Environment**: All (staging, production)
- **How to obtain**: Created along with `AWS_ACCESS_KEY_ID` above

### Environment-Specific Secrets

These secrets should be configured for each environment (staging/production) using GitHub Environment Secrets.

#### `EKS_CLUSTER_NAME`
- **Description**: Name of the EKS cluster
- **Required**: Yes
- **Staging Value**: `core-staging-eks`
- **Production Value**: `core-production-eks`

#### `APP_URL`
- **Description**: Public URL of the application (for smoke tests)
- **Required**: Yes
- **Staging Example**: `https://staging.yourdomain.com`
- **Production Example**: `https://app.yourdomain.com`

#### `API_URL`
- **Description**: Public URL of the API (for health checks)
- **Required**: Yes
- **Staging Example**: `https://api-staging.yourdomain.com`
- **Production Example**: `https://api.yourdomain.com`

### Optional Secrets

#### `SONAR_TOKEN`
- **Description**: SonarCloud authentication token for code quality analysis
- **Required**: No (only if using SonarCloud)
- **How to obtain**: Create token at https://sonarcloud.io/account/security

#### `SLACK_WEBHOOK_URL`
- **Description**: Slack webhook URL for deployment notifications
- **Required**: No (only if you want Slack notifications)
- **How to obtain**: Create incoming webhook in Slack workspace settings

## IAM Policies

The GitHub Actions deployer IAM user requires the following permissions:

### Recommended Policy (Least Privilege)

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "ECRPermissions",
      "Effect": "Allow",
      "Action": [
        "ecr:GetAuthorizationToken",
        "ecr:BatchCheckLayerAvailability",
        "ecr:GetDownloadUrlForLayer",
        "ecr:BatchGetImage",
        "ecr:PutImage",
        "ecr:InitiateLayerUpload",
        "ecr:UploadLayerPart",
        "ecr:CompleteLayerUpload"
      ],
      "Resource": "*"
    },
    {
      "Sid": "EKSPermissions",
      "Effect": "Allow",
      "Action": [
        "eks:DescribeCluster",
        "eks:ListClusters"
      ],
      "Resource": "*"
    },
    {
      "Sid": "TerraformStatePermissions",
      "Effect": "Allow",
      "Action": [
        "s3:ListBucket",
        "s3:GetObject",
        "s3:PutObject",
        "s3:DeleteObject"
      ],
      "Resource": [
        "arn:aws:s3:::core-terraform-state",
        "arn:aws:s3:::core-terraform-state/*"
      ]
    },
    {
      "Sid": "TerraformLockPermissions",
      "Effect": "Allow",
      "Action": [
        "dynamodb:GetItem",
        "dynamodb:PutItem",
        "dynamodb:DeleteItem"
      ],
      "Resource": "arn:aws:dynamodb:*:*:table/core-terraform-locks"
    },
    {
      "Sid": "TerraformResourcePermissions",
      "Effect": "Allow",
      "Action": [
        "ec2:*",
        "rds:*",
        "elasticache:*",
        "kms:*",
        "iam:*",
        "logs:*",
        "secretsmanager:*",
        "cloudwatch:*",
        "autoscaling:*"
      ],
      "Resource": "*"
    }
  ]
}
```

### Alternative: Use AWS Managed Policies

For simplicity (though less secure), you can attach these managed policies:
- `PowerUserAccess` (allows most AWS services except IAM)
- `IAMFullAccess` (for IAM operations needed by Terraform)

## Setting Up GitHub Secrets

### Repository Secrets

1. Navigate to your GitHub repository
2. Go to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add each secret from the list above

### Environment Secrets

1. Navigate to your GitHub repository
2. Go to **Settings** → **Environments**
3. Create two environments: `staging` and `production`
4. For each environment:
   - Click on the environment name
   - Click **Add secret**
   - Add environment-specific secrets

## Verification

To verify secrets are configured correctly:

1. **Test AWS Credentials**:
   ```bash
   # Use the AWS credentials to verify access
   export AWS_ACCESS_KEY_ID="your_access_key"
   export AWS_SECRET_ACCESS_KEY="your_secret_key"
   aws sts get-caller-identity
   ```

2. **Test ECR Access**:
   ```bash
   aws ecr get-login-password --region us-east-1 | \
     docker login --username AWS --password-stdin \
     $(aws sts get-caller-identity --query Account --output text).dkr.ecr.us-east-1.amazonaws.com
   ```

3. **Test EKS Access**:
   ```bash
   aws eks describe-cluster --name core-staging-eks --region us-east-1
   ```

## Security Best Practices

1. **Rotate Credentials Regularly**: Rotate AWS access keys every 90 days
2. **Use Least Privilege**: Grant only the minimum permissions required
3. **Monitor Usage**: Enable CloudTrail to monitor API calls
4. **Use Environment Secrets**: Store environment-specific values in environment secrets, not repository secrets
5. **Audit Access**: Regularly review who has access to GitHub secrets
6. **Enable Branch Protection**: Require PR reviews before deploying to production

## Troubleshooting

### Error: "The security token included in the request is invalid"
- **Cause**: AWS credentials are incorrect or expired
- **Solution**: Verify `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY` are correct

### Error: "User is not authorized to perform: eks:DescribeCluster"
- **Cause**: IAM user lacks necessary permissions
- **Solution**: Attach the required IAM policies to the user

### Error: "Unable to locate credentials"
- **Cause**: AWS secrets are not set in GitHub
- **Solution**: Verify secrets are configured in GitHub Settings → Secrets

## Next Steps

After configuring GitHub secrets:
1. Run the [AWS Deployment Guide](aws-deployment.md)
2. Test the CI/CD pipeline with a manual workflow trigger
3. Review the [Pre-Deployment Checklist](pre-deployment-checklist.md)

## Support

For issues or questions:
- Review GitHub Actions workflow logs
- Check AWS CloudTrail for API call details
- Verify IAM permissions in AWS Console

