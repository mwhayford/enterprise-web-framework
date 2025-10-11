# AWS Deployment Guide

This guide provides step-by-step instructions for deploying the Core application to AWS using Terraform and Kubernetes.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Architecture Overview](#architecture-overview)
- [Initial Setup](#initial-setup)
- [Deploy to Staging](#deploy-to-staging)
- [Deploy to Production](#deploy-to-production)
- [Post-Deployment](#post-deployment)
- [Monitoring and Maintenance](#monitoring-and-maintenance)
- [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Tools

Install the following tools on your local machine:

1. **AWS CLI** (v2.x or later)
   ```bash
   # macOS
   brew install awscli
   
   # Windows
   winget install Amazon.AWSCLI
   
   # Verify installation
   aws --version
   ```

2. **Terraform** (v1.6.0 or later)
   ```bash
   # macOS
   brew install terraform
   
   # Windows
   winget install Hashicorp.Terraform
   
   # Verify installation
   terraform --version
   ```

3. **kubectl** (v1.28.x or later)
   ```bash
   # macOS
   brew install kubectl
   
   # Windows
   winget install Kubernetes.kubectl
   
   # Verify installation
   kubectl version --client
   ```

4. **Docker** (for building images locally)
   - Install Docker Desktop from https://www.docker.com/products/docker-desktop

### AWS Account Setup

1. **AWS Account**: You need an AWS account with billing enabled
2. **IAM User**: Create an IAM user with appropriate permissions (see [GitHub Secrets](github-secrets.md))
3. **AWS Credentials**: Configure AWS CLI with your credentials
   ```bash
   aws configure
   ```

### GitHub Repository Setup

1. Fork/clone the repository
2. Configure GitHub secrets (see [GitHub Secrets](github-secrets.md))
3. Enable GitHub Actions in repository settings

## Architecture Overview

The deployment creates the following AWS infrastructure:

### Networking
- **VPC**: Custom VPC with public, private, and database subnets across 3 AZs
- **NAT Gateways**: For outbound internet access from private subnets
- **Security Groups**: Tightly scoped security groups for each component

### Compute
- **EKS Cluster**: Managed Kubernetes cluster (v1.28)
- **Node Groups**: Auto-scaling EC2 instances for running workloads
- **ECR Repositories**: Private Docker registries for container images

### Data
- **RDS PostgreSQL**: Managed relational database (v16)
- **ElastiCache Redis**: In-memory cache and session store
- **S3**: Terraform state storage

### Security
- **KMS Keys**: Encryption at rest for RDS, Redis, and EKS secrets
- **Secrets Manager**: Secure storage for database credentials
- **IAM Roles**: Fine-grained access control with IRSA

### Monitoring
- **CloudWatch**: Logs, metrics, and alarms
- **VPC Flow Logs**: Network traffic analysis (production)

## Initial Setup

### Step 1: Configure Terraform Backend

Run the setup script to create S3 bucket and DynamoDB table for Terraform state:

```bash
# Linux/macOS
cd infrastructure/terraform
chmod +x setup-backend.sh
./setup-backend.sh

# Windows PowerShell
cd infrastructure\terraform
.\setup-backend.ps1
```

This creates:
- S3 bucket: `core-terraform-state`
- DynamoDB table: `core-terraform-locks`

### Step 2: Create ECR Repositories (Optional)

If not using Terraform to create ECR repositories, create them manually:

```bash
aws ecr create-repository --repository-name core-backend --region us-east-1
aws ecr create-repository --repository-name core-frontend --region us-east-1
```

### Step 3: Review Configuration

Review and customize the Terraform variables:

**Staging**: `infrastructure/terraform/environments/staging/variables.tf`
**Production**: `infrastructure/terraform/environments/production/variables.tf`

## Deploy to Staging

### Step 1: Initialize Terraform

```bash
cd infrastructure/terraform/environments/staging
terraform init
```

### Step 2: Plan Infrastructure

```bash
terraform plan -out=tfplan
```

Review the plan carefully. Terraform will create:
- 1 VPC with subnets
- 1 EKS cluster with node group
- 1 RDS PostgreSQL instance
- 1 ElastiCache Redis cluster
- Security groups, KMS keys, IAM roles, etc.

### Step 3: Apply Infrastructure

```bash
terraform apply tfplan
```

**Time**: This takes approximately 15-20 minutes.

### Step 4: Configure kubectl

```bash
aws eks update-kubeconfig \
  --name core-staging-eks \
  --region us-east-1
  
# Verify access
kubectl get nodes
```

### Step 5: Install ALB Controller

Install AWS Load Balancer Controller for ingress:

```bash
# Download IAM policy
curl -o iam-policy.json https://raw.githubusercontent.com/kubernetes-sigs/aws-load-balancer-controller/v2.6.0/docs/install/iam_policy.json

# Create IAM policy
aws iam create-policy \
  --policy-name AWSLoadBalancerControllerIAMPolicy \
  --policy-document file://iam-policy.json

# Install using Helm
helm repo add eks https://aws.github.io/eks-charts
helm install aws-load-balancer-controller eks/aws-load-balancer-controller \
  -n kube-system \
  --set clusterName=core-staging-eks \
  --set serviceAccount.create=true \
  --set serviceAccount.name=aws-load-balancer-controller
```

### Step 6: Install Metrics Server

Install metrics server for HPA:

```bash
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
```

### Step 7: Deploy Application

```bash
cd ../../../../infrastructure/kubernetes
kubectl apply -k overlays/staging
```

### Step 8: Verify Deployment

```bash
# Check pods
kubectl get pods -n core-staging

# Check services
kubectl get svc -n core-staging

# Check ingress
kubectl get ingress -n core-staging

# Get ALB URL
kubectl get ingress core-ingress -n core-staging -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'
```

### Step 9: Run Database Migrations

```bash
kubectl exec -n core-staging deployment/core-backend -- \
  dotnet ef database update --project Core.Infrastructure --startup-project Core.API
```

### Step 10: Test Application

```bash
# Get Load Balancer URL
ALB_URL=$(kubectl get ingress core-ingress -n core-staging -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')

# Test health endpoint
curl http://$ALB_URL/health

# Test API
curl http://$ALB_URL/api/health
```

## Deploy to Production

Production deployment follows the same steps as staging, but with production-specific configurations:

### Key Differences

- **High Availability**: Multi-AZ deployments for RDS and Redis
- **More Resources**: Larger instance types and more replicas
- **Deletion Protection**: Enabled for critical resources
- **Monitoring**: Enhanced monitoring and alerting
- **Backups**: 30-day retention for RDS backups

### Deployment Steps

```bash
# Navigate to production environment
cd infrastructure/terraform/environments/production

# Initialize Terraform
terraform init

# Plan infrastructure
terraform plan -out=tfplan

# Review plan carefully!
# Production deployment is more expensive and has stricter settings

# Apply infrastructure
terraform apply tfplan

# Configure kubectl
aws eks update-kubeconfig \
  --name core-production-eks \
  --region us-east-1

# Install ALB Controller (same as staging)
# Install Metrics Server (same as staging)

# Deploy application
cd ../../../../infrastructure/kubernetes
kubectl apply -k overlays/production

# Verify deployment
kubectl get pods -n core-production
kubectl get svc -n core-production
kubectl get ingress -n core-production
```

## Post-Deployment

### Configure DNS

1. Get the ALB DNS name:
   ```bash
   kubectl get ingress core-ingress -n core-production -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'
   ```

2. Create CNAME records in Route 53 (or your DNS provider):
   - `api.yourdomain.com` → ALB DNS name
   - `app.yourdomain.com` → ALB DNS name

### Configure SSL/TLS

1. Request certificate in AWS Certificate Manager:
   ```bash
   aws acm request-certificate \
     --domain-name "*.yourdomain.com" \
     --validation-method DNS \
     --region us-east-1
   ```

2. Add DNS validation records in Route 53

3. Update ingress to use the certificate:
   ```yaml
   annotations:
     alb.ingress.kubernetes.io/certificate-arn: arn:aws:acm:us-east-1:xxxxx:certificate/xxxxx
   ```

### Update Application Secrets

1. Update Kubernetes secrets with production values:
   ```bash
   kubectl create secret generic core-secrets \
     --from-literal=ConnectionStrings__DefaultConnection="Host=..." \
     --from-literal=JwtSettings__SecretKey="..." \
     --from-literal=StripeSettings__SecretKey="..." \
     --namespace core-production \
     --dry-run=client -o yaml | kubectl apply -f -
   ```

2. Restart deployments to pick up new secrets:
   ```bash
   kubectl rollout restart deployment/core-backend -n core-production
   kubectl rollout restart deployment/core-frontend -n core-production
   ```

## Monitoring and Maintenance

### CloudWatch Dashboards

1. Navigate to CloudWatch in AWS Console
2. Create custom dashboards for:
   - EKS cluster metrics
   - RDS performance metrics
   - Redis metrics
   - Application logs

### Set Up Alerts

Create CloudWatch alarms for:
- High CPU utilization (>80%)
- Low disk space (<10 GB)
- Failed health checks
- High error rates

### Regular Maintenance

- **Weekly**: Review CloudWatch metrics and logs
- **Monthly**: Apply Kubernetes security patches
- **Quarterly**: Review and optimize costs
- **Annually**: Rotate credentials and certificates

## Troubleshooting

### Pods Not Starting

```bash
# Check pod status
kubectl get pods -n core-staging

# Describe pod for events
kubectl describe pod <pod-name> -n core-staging

# Check logs
kubectl logs <pod-name> -n core-staging
```

### Database Connection Issues

```bash
# Test connection from pod
kubectl exec -it <pod-name> -n core-staging -- /bin/bash
apt-get update && apt-get install -y postgresql-client
psql -h <rds-endpoint> -U postgres -d CoreDb
```

### ALB Not Working

```bash
# Check ALB controller logs
kubectl logs -n kube-system \
  -l app.kubernetes.io/name=aws-load-balancer-controller

# Check ingress status
kubectl describe ingress core-ingress -n core-staging
```

### Terraform State Locked

```bash
# List locks
aws dynamodb scan --table-name core-terraform-locks

# Force unlock (if needed)
terraform force-unlock <lock-id>
```

## Cost Optimization

### Staging Environment
- Use SPOT instances (~70% savings)
- Single NAT Gateway (~$32/month savings)
- Single-AZ RDS (~50% savings)
- Smaller instance types
- **Estimated Cost**: $200-300/month

### Production Environment
- Use ON_DEMAND instances (reliability)
- Multi-AZ NAT Gateways (high availability)
- Multi-AZ RDS (high availability)
- Larger instance types for performance
- **Estimated Cost**: $1000-1500/month

### Cost Reduction Tips
1. Use Reserved Instances for steady-state workloads
2. Enable auto-scaling to scale down during low traffic
3. Use Savings Plans for compute resources
4. Delete unused EBS volumes and snapshots
5. Use S3 lifecycle policies to archive old logs

## Rollback Procedures

### Application Rollback

```bash
# View rollout history
kubectl rollout history deployment/core-backend -n core-staging

# Rollback to previous version
kubectl rollout undo deployment/core-backend -n core-staging

# Rollback to specific revision
kubectl rollout undo deployment/core-backend --to-revision=2 -n core-staging
```

### Infrastructure Rollback

```bash
# Terraform state rollback (use with caution!)
terraform state list
terraform state pull > backup.tfstate

# Apply previous Terraform configuration
git checkout <previous-commit>
terraform apply
```

## Next Steps

1. Set up CI/CD automation (see [CI/CD Documentation](ci-cd.md))
2. Configure monitoring and alerting
3. Implement backup and disaster recovery procedures
4. Perform load testing
5. Create runbooks for common operations

## Support

For issues or questions:
- Review CloudWatch logs
- Check EKS cluster status in AWS Console
- Verify security groups and network connectivity
- Review Terraform state for infrastructure issues


