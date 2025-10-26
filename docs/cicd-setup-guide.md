# CI/CD Setup Guide - GitHub Actions to AWS

Complete guide to set up automated deployment from GitHub Actions to AWS.

## Overview

This guide will help you configure:
1. ✅ Terraform backend (S3 + DynamoDB)
2. ✅ Initialize and test Terraform infrastructure
3. ✅ Configure GitHub repository secrets
4. ✅ Set up GitHub environments (staging/production)
5. ✅ Test CI/CD pipeline
6. ✅ Deploy application

**Estimated Time**: 30-45 minutes

---

## Prerequisites Checklist

- [x] AWS CLI installed and configured
- [x] Terraform installed
- [x] kubectl installed
- [x] Docker installed
- [x] Git installed
- [x] AWS IAM user created with appropriate permissions
- [ ] GitHub repository with admin access
- [ ] AWS credentials saved securely

**Your AWS Account**: 436399375303

---

## Phase 1: Setup Terraform Backend (5 minutes)

The Terraform backend stores infrastructure state in S3 and uses DynamoDB for locking.

### Step 1: Run Backend Setup Script

```powershell
# Navigate to Terraform directory
cd infrastructure\terraform

# Run the setup script
.\setup-backend.ps1

# Expected output:
# ✓ S3 bucket created: core-terraform-state
# ✓ DynamoDB table created: core-terraform-locks
```

This creates:
- **S3 Bucket**: `core-terraform-state` (versioned, encrypted)
- **DynamoDB Table**: `core-terraform-locks` (for state locking)

### Step 2: Verify Backend Resources

```powershell
# Verify S3 bucket
aws s3 ls | findstr core-terraform-state

# Verify DynamoDB table
aws dynamodb describe-table --table-name core-terraform-locks --query "Table.TableStatus"
```

---

## Phase 2: Initialize Terraform (10 minutes)

### Step 1: Navigate to Staging Environment

```powershell
cd environments\staging
```

### Step 2: Review Configuration (Optional)

Review the Terraform configuration:
- `main.tf` - Infrastructure resources
- `variables.tf` - Input variables
- `outputs.tf` - Output values

### Step 3: Initialize Terraform

```powershell
terraform init
```

Expected output:
```
Initializing the backend...
Initializing modules...
Initializing provider plugins...
Terraform has been successfully initialized!
```

### Step 4: Validate Configuration

```powershell
terraform validate
```

Should return: `Success! The configuration is valid.`

### Step 5: Plan Infrastructure (Review Only - Don't Apply Yet)

```powershell
terraform plan
```

This shows what will be created:
- VPC with subnets across 3 AZs
- EKS cluster with node group
- RDS PostgreSQL database
- ElastiCache Redis cluster
- Security groups, IAM roles, KMS keys
- ECR repositories

**Expected Resources**: ~50-60 resources
**Estimated Cost**: $200-300/month for staging

⚠️ **Don't run `terraform apply` yet** - we'll do this after GitHub setup

---

## Phase 3: Configure GitHub Repository (15 minutes)

### Step 1: Push Code to GitHub (If Not Done)

```powershell
# Navigate to project root
cd ..\..\..

# Check git status
git status

# Push to GitHub (if you haven't already)
git push origin main
```

### Step 2: Configure Repository Secrets

Go to your GitHub repository:
1. Navigate to **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**

**Add these secrets**:

#### AWS_ACCESS_KEY_ID
```
Value: <Your AWS Access Key ID>
Source: From AWS IAM when you created the user
```

#### AWS_SECRET_ACCESS_KEY
```
Value: <Your AWS Secret Access Key>
Source: From AWS IAM when you created the user
```

### Step 3: Create GitHub Environments

Create two environments for staging and production:

#### Create Staging Environment
1. Go to **Settings** → **Environments**
2. Click **New environment**
3. Name: `staging`
4. Click **Configure environment**
5. Add environment secrets:

| Secret Name | Value |
|-------------|-------|
| `EKS_CLUSTER_NAME` | `core-staging-eks` |
| `APP_URL` | `http://your-staging-alb-url.com` (will update after deployment) |
| `API_URL` | `http://your-staging-alb-url.com` (will update after deployment) |

6. Click **Add secret** for each

#### Create Production Environment
1. Click **New environment**
2. Name: `production`
3. Configure **Environment protection rules**:
   - ✅ Check "Required reviewers"
   - Add yourself or team members as reviewers
   - (Optional) Set wait timer: 5 minutes
4. Add environment secrets:

| Secret Name | Value |
|-------------|-------|
| `EKS_CLUSTER_NAME` | `core-production-eks` |
| `APP_URL` | `http://your-production-domain.com` (will update after deployment) |
| `API_URL` | `http://your-production-domain.com` (will update after deployment) |

---

## Phase 4: Test Terraform Deployment Locally (10 minutes)

Before triggering CI/CD, let's test Terraform locally.

### Step 1: Deploy Staging Infrastructure

```powershell
# Navigate to staging environment
cd infrastructure\terraform\environments\staging

# Create execution plan
terraform plan -out=tfplan

# Review the plan carefully!
# This will create real AWS resources and incur costs

# Apply the plan (if everything looks good)
terraform apply tfplan
```

⏱️ **This takes 15-20 minutes** (EKS cluster creation is slow)

During deployment, Terraform will:
1. Create VPC and networking (2-3 min)
2. Create EKS cluster (10-15 min)
3. Create RDS database (5-7 min)
4. Create Redis cluster (3-5 min)
5. Create ECR repositories (1 min)

### Step 2: Save Terraform Outputs

After successful deployment, save the outputs:

```powershell
# Get all outputs
terraform output

# Save specific outputs for later use
terraform output -raw eks_cluster_name
terraform output -raw rds_endpoint
terraform output -raw redis_endpoint
terraform output -raw ecr_backend_repository_url
terraform output -raw ecr_frontend_repository_url
```

**Save these values** - you'll need them for Kubernetes deployment.

---

## Phase 5: Configure kubectl for EKS (5 minutes)

### Step 1: Update kubeconfig

```powershell
aws eks update-kubeconfig --name core-staging-eks --region us-east-1
```

### Step 2: Verify Cluster Access

```powershell
# Check nodes
kubectl get nodes

# Expected output: 2 nodes in Ready state
```

### Step 3: Install Required Kubernetes Components

#### Install AWS Load Balancer Controller

```powershell
# Add Helm repo
helm repo add eks https://aws.github.io/eks-charts
helm repo update

# Get your AWS account ID
$AWS_ACCOUNT_ID = aws sts get-caller-identity --query Account --output text

# Install the controller
helm install aws-load-balancer-controller eks/aws-load-balancer-controller `
  -n kube-system `
  --set clusterName=core-staging-eks `
  --set serviceAccount.create=true `
  --set serviceAccount.name=aws-load-balancer-controller
```

#### Install Metrics Server

```powershell
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
```

### Step 4: Verify Components

```powershell
# Check ALB controller
kubectl get deployment -n kube-system aws-load-balancer-controller

# Check metrics server
kubectl get deployment -n kube-system metrics-server
```

---

## Phase 6: Deploy Application to Kubernetes (5 minutes)

### Step 1: Update Kubernetes Secrets

First, get the RDS password from AWS Secrets Manager:

```powershell
# Get RDS secret ARN
$RDS_SECRET_ARN = terraform output -raw rds_secret_arn

# Get database connection details
aws secretsmanager get-secret-value --secret-id $RDS_SECRET_ARN --query SecretString --output text
```

### Step 2: Create Kubernetes Secrets

```powershell
# Navigate to project root
cd ..\..\..\..\

# Get database details from Terraform output
cd infrastructure\terraform\environments\staging
$DB_HOST = terraform output -raw rds_address
$DB_NAME = terraform output -raw rds_database_name
$REDIS_HOST = terraform output -raw redis_endpoint

# Create secrets (replace PASSWORD with actual value from Secrets Manager)
cd ..\..\..\..\
kubectl create secret generic core-secrets `
  --from-literal=ConnectionStrings__DefaultConnection="Host=$DB_HOST;Port=5432;Database=$DB_NAME;Username=postgres;Password=YOUR_PASSWORD_HERE" `
  --from-literal=Redis__Configuration="$REDIS_HOST:6379" `
  --from-literal=JwtSettings__SecretKey="YOUR_JWT_SECRET_HERE" `
  --from-literal=StripeSettings__SecretKey="YOUR_STRIPE_SECRET_HERE" `
  --from-literal=Authentication__Google__ClientSecret="YOUR_GOOGLE_CLIENT_SECRET" `
  --namespace core-staging `
  --dry-run=client -o yaml | kubectl apply -f -
```

### Step 3: Deploy Application

```powershell
cd infrastructure\kubernetes
kubectl apply -k overlays/staging
```

### Step 4: Verify Deployment

```powershell
# Check pods
kubectl get pods -n core-staging

# Wait for pods to be ready (may take 2-3 minutes)
kubectl wait --for=condition=ready pod -l app=core-backend -n core-staging --timeout=300s

# Check services
kubectl get svc -n core-staging

# Get ALB URL
kubectl get ingress core-ingress -n core-staging
```

### Step 5: Run Database Migrations

```powershell
# Run migrations
kubectl exec -n core-staging deployment/core-backend -- `
  dotnet ef database update --project RentalManager.Infrastructure --startup-project RentalManager.API

# Verify database
kubectl exec -n core-staging deployment/core-backend -- `
  dotnet ef migrations list --project RentalManager.Infrastructure --startup-project RentalManager.API
```

### Step 6: Test Application

```powershell
# Get ALB DNS name
$ALB_URL = kubectl get ingress core-ingress -n core-staging -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'

# Test health endpoint
curl "http://$ALB_URL/health"

# Test API health
curl "http://$ALB_URL/api/health"
```

---

## Phase 7: Test CI/CD Pipeline (5 minutes)

Now that infrastructure is set up, let's test the CI/CD pipeline.

### Step 1: Push Docker Images to ECR

```powershell
# Get ECR repository URLs
cd infrastructure\terraform\environments\staging
$ECR_BACKEND = terraform output -raw ecr_backend_repository_url
$ECR_FRONTEND = terraform output -raw ecr_frontend_repository_url
$AWS_REGION = "us-east-1"
$AWS_ACCOUNT_ID = aws sts get-caller-identity --query Account --output text

# Login to ECR
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com"

# Build and push backend
cd ..\..\..\..\src\backend
docker build -t $ECR_BACKEND`:latest -f Dockerfile .
docker push $ECR_BACKEND`:latest

# Build and push frontend
cd ..\frontend
docker build -t $ECR_FRONTEND`:latest -f Dockerfile .
docker push $ECR_FRONTEND`:latest
```

### Step 2: Trigger CI Pipeline

Make a small change to trigger CI:

```powershell
# Navigate to project root
cd ..\..

# Make a small change
echo "# CI/CD Test" >> README.md

# Commit and push
git add README.md
git commit -m "test: trigger CI pipeline"
git push origin main
```

### Step 3: Monitor CI Pipeline

1. Go to your GitHub repository
2. Click **Actions** tab
3. You should see "CI Pipeline" running
4. Click on the workflow run to see progress

The CI pipeline will:
- ✅ Build and test backend (.NET)
- ✅ Build and test frontend (React)
- ✅ Run E2E tests
- ✅ Run security scans
- ✅ Build Docker images

### Step 4: Trigger CD Pipeline (Manual)

The CD pipeline is manually triggered:

1. Go to **Actions** tab
2. Click **CD Pipeline** workflow
3. Click **Run workflow**
4. Select:
   - Environment: `staging`
   - Version: `v1.0.0`
5. Click **Run workflow**

The CD pipeline will:
- ✅ Build and push Docker images to ECR
- ✅ Deploy to Kubernetes
- ✅ Run database migrations
- ✅ Run smoke tests
- ✅ Send notifications

---

## Phase 8: Verify Everything Works (5 minutes)

### Checklist

- [ ] AWS infrastructure deployed successfully
- [ ] EKS cluster accessible via kubectl
- [ ] Application pods running in Kubernetes
- [ ] Database migrations applied
- [ ] Application accessible via ALB
- [ ] CI pipeline passing on GitHub
- [ ] CD pipeline successfully deploys
- [ ] Health endpoints returning 200 OK
- [ ] Logs visible in CloudWatch

### Test User Flow

1. Open browser to ALB URL
2. Register a new user
3. Login with Google OAuth (if configured)
4. Test payment flow (test mode)
5. Test search functionality

---

## Troubleshooting

### Issue: Terraform State Lock

```powershell
# List locks
aws dynamodb scan --table-name core-terraform-locks

# Force unlock (if needed)
terraform force-unlock <LOCK_ID>
```

### Issue: kubectl Cannot Connect to Cluster

```powershell
# Update kubeconfig
aws eks update-kubeconfig --name core-staging-eks --region us-east-1

# Verify
kubectl cluster-info
```

### Issue: Pods Not Starting

```powershell
# Check pod status
kubectl get pods -n core-staging

# Describe pod
kubectl describe pod <POD_NAME> -n core-staging

# Check logs
kubectl logs <POD_NAME> -n core-staging
```

### Issue: ALB Not Created

```powershell
# Check ALB controller logs
kubectl logs -n kube-system -l app.kubernetes.io/name=aws-load-balancer-controller

# Check ingress
kubectl describe ingress core-ingress -n core-staging
```

---

## Next Steps

After staging is working:

1. **Configure DNS**: Point your domain to the ALB
2. **Setup SSL**: Use AWS Certificate Manager
3. **Deploy to Production**: Follow same steps for production environment
4. **Setup Monitoring**: Configure CloudWatch dashboards and alarms
5. **Setup Alerting**: Configure SNS topics for critical alerts

---

## Cost Monitoring

Monitor your AWS costs:

```powershell
# Get current month costs
aws ce get-cost-and-usage `
  --time-period Start=2025-10-01,End=2025-10-31 `
  --granularity MONTHLY `
  --metrics "UnblendedCost" `
  --group-by Type=DIMENSION,Key=SERVICE
```

**Expected Monthly Costs**:
- Staging: ~$200-300
- Production: ~$1000-1500

---

## Clean Up (Optional)

To destroy staging environment:

```powershell
cd infrastructure\terraform\environments\staging

# Destroy Kubernetes resources first
cd ..\..\..\..\infrastructure\kubernetes
kubectl delete -k overlays/staging

# Destroy Terraform infrastructure
cd ..\terraform\environments\staging
terraform destroy
```

⚠️ **Warning**: This will delete all resources and data!

---

## Support

- Review logs in CloudWatch
- Check GitHub Actions logs
- Verify AWS resource states in Console
- Review Kubernetes events: `kubectl get events -n core-staging`


