# Staging Environment Test Workflow

This guide walks through deploying, testing, and completely tearing down the staging infrastructure.

## Overview

**Purpose**: Test deployment process without incurring ongoing costs  
**Cost**: Pay only for hours used during testing (~$0.25/hour)  
**Cleanup**: Complete teardown returns cost to $0/month (except $2 for state storage)

---

## Prerequisites

- [ ] AWS credentials configured
- [ ] Terraform installed
- [ ] kubectl installed
- [ ] Docker installed
- [ ] AWS CLI configured

---

## Phase 1: Deploy Infrastructure (20-25 minutes)

### Step 1: Initialize Terraform (if not done)

```bash
cd infrastructure/terraform/environments/staging
terraform init
```

### Step 2: Review Plan

```bash
terraform plan -out=tfplan
```

**Expected resources**: 66 resources to be created

### Step 3: Apply Infrastructure

```bash
terraform apply tfplan
```

**What happens:**
- Creates VPC, subnets, NAT gateway (~5 min)
- Creates EKS cluster (~10 min)
- Creates EKS node group (~8 min)
- Creates RDS instance (~5 min)
- Creates ElastiCache (~5 min)
- Creates ECR repositories (~1 min)

**Estimated time**: 20-25 minutes  
**Cost so far**: ~$0.15

### Step 4: Save Important Outputs

```bash
terraform output > ../../outputs.txt
```

Key outputs:
- `eks_cluster_name`
- `ecr_backend_repository_url`
- `ecr_frontend_repository_url`
- `rds_endpoint`
- `redis_endpoint`

---

## Phase 2: Configure kubectl (2-3 minutes)

### Step 1: Update kubeconfig

```bash
aws eks update-kubeconfig \
  --region us-east-1 \
  --name core-staging-eks
```

### Step 2: Verify Connection

```bash
kubectl get nodes
kubectl get pods --all-namespaces
```

**Expected**: 2 nodes in "Ready" state

**Cost so far**: ~$0.18

---

## Phase 3: Build and Push Docker Images (5-10 minutes)

### Step 1: Get ECR Repository URLs

```bash
cd ../../../..  # Back to project root

# Set variables
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
REGION=us-east-1
BACKEND_REPO="${ACCOUNT_ID}.dkr.ecr.${REGION}.amazonaws.com/core-backend"
FRONTEND_REPO="${ACCOUNT_ID}.dkr.ecr.${REGION}.amazonaws.com/core-frontend"

echo "Backend: $BACKEND_REPO"
echo "Frontend: $FRONTEND_REPO"
```

### Step 2: Login to ECR

```bash
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin ${ACCOUNT_ID}.dkr.ecr.us-east-1.amazonaws.com
```

### Step 3: Build and Push Backend

```bash
cd src/backend

# Build
docker build -t core-backend -f Dockerfile .

# Tag
docker tag core-backend:latest ${BACKEND_REPO}:latest
docker tag core-backend:latest ${BACKEND_REPO}:v1.0.0

# Push
docker push ${BACKEND_REPO}:latest
docker push ${BACKEND_REPO}:v1.0.0

cd ../..
```

### Step 4: Build and Push Frontend

```bash
cd src/frontend

# Build
docker build -t core-frontend -f Dockerfile .

# Tag
docker tag core-frontend:latest ${FRONTEND_REPO}:latest
docker tag core-frontend:latest ${FRONTEND_REPO}:v1.0.0

# Push
docker push ${FRONTEND_REPO}:latest
docker push ${FRONTEND_REPO}:v1.0.0

cd ../..
```

**Cost so far**: ~$0.30

---

## Phase 4: Deploy Application to Kubernetes (10-15 minutes)

### Step 1: Create Kubernetes Namespace

```bash
kubectl create namespace core-staging
```

### Step 2: Create Secrets

Get database password from Secrets Manager:

```bash
# Get RDS secret ARN from Terraform outputs
SECRET_ARN=$(cd infrastructure/terraform/environments/staging && terraform output -raw rds_secret_arn)

# Get database password
DB_PASSWORD=$(aws secretsmanager get-secret-value \
  --secret-id $SECRET_ARN \
  --query SecretString --output text | \
  jq -r .password)

# Get connection details from terraform outputs
DB_HOST=$(cd infrastructure/terraform/environments/staging && terraform output -raw rds_endpoint)
REDIS_HOST=$(cd infrastructure/terraform/environments/staging && terraform output -raw redis_endpoint)
```

Create Kubernetes secret:

```bash
kubectl create secret generic core-secrets -n core-staging \
  --from-literal=database-password=$DB_PASSWORD \
  --from-literal=jwt-key=YourSuperSecretKeyThatIsAtLeast32CharactersLong! \
  --from-literal=stripe-secret-key=sk_test_dummy_key_for_testing \
  --from-literal=stripe-publishable-key=pk_test_dummy_key_for_testing \
  --from-literal=google-client-id=test-google-client-id \
  --from-literal=google-client-secret=test-google-client-secret
```

### Step 3: Update Kubernetes Manifests

Edit `infrastructure/kubernetes/overlays/staging/kustomization.yaml` to use your ECR images:

```yaml
images:
  - name: backend
    newName: ${ACCOUNT_ID}.dkr.ecr.us-east-1.amazonaws.com/core-backend
    newTag: v1.0.0
  - name: frontend
    newName: ${ACCOUNT_ID}.dkr.ecr.us-east-1.amazonaws.com/core-frontend
    newTag: v1.0.0
```

Update ConfigMap with actual endpoints:

```yaml
configMapGenerator:
  - name: core-config
    literals:
      - DATABASE_HOST=${DB_HOST}
      - REDIS_HOST=${REDIS_HOST}
```

### Step 4: Apply Kubernetes Manifests

```bash
kubectl apply -k infrastructure/kubernetes/overlays/staging
```

### Step 5: Wait for Deployments

```bash
# Watch deployment progress
kubectl get pods -n core-staging -w

# Check deployment status
kubectl rollout status deployment/core-backend -n core-staging
kubectl rollout status deployment/core-frontend -n core-staging
```

### Step 6: Run Database Migrations

```bash
# Get backend pod name
BACKEND_POD=$(kubectl get pod -n core-staging -l app=core-backend -o jsonpath="{.items[0].metadata.name}")

# Run migrations
kubectl exec -n core-staging $BACKEND_POD -- dotnet ef database update
```

**Cost so far**: ~$0.60

---

## Phase 5: Test Application (1-2 hours)

### Step 1: Get Load Balancer URL

```bash
kubectl get ingress -n core-staging
# Or if using LoadBalancer service:
kubectl get svc -n core-staging
```

### Step 2: Test Endpoints

```bash
# Health check
curl http://<load-balancer-url>/health

# API endpoint
curl http://<load-balancer-url>/api/users
```

### Step 3: Test Frontend

Open in browser:
```
http://<load-balancer-url>
```

### Step 4: Run E2E Tests (Optional)

Update `BASE_URL` in playwright config:

```bash
cd tests/Core.E2ETests
BASE_URL=http://<load-balancer-url> npx playwright test
```

### Step 5: Verify Integrations

- [ ] Database connection works
- [ ] Redis caching works
- [ ] Elasticsearch indexing works
- [ ] Kafka messaging works
- [ ] Stripe test payments work
- [ ] Google OAuth (if configured)
- [ ] Metrics/logging

**Testing time**: 1-2 hours  
**Cost after 2 hours**: ~$1.00

---

## Phase 6: Complete Teardown (10-15 minutes)

### Step 1: Export Any Important Data (Optional)

```bash
# Export database backup
aws rds create-db-snapshot \
  --db-instance-identifier core-staging-db \
  --db-snapshot-identifier staging-final-$(date +%Y%m%d)

# Export logs
kubectl logs -n core-staging --all-containers --tail=1000 > staging-logs.txt
```

### Step 2: Delete Kubernetes Resources First

```bash
# Delete all resources in namespace
kubectl delete namespace core-staging

# Verify deletion
kubectl get all -n core-staging
```

### Step 3: Plan Destruction

```bash
cd infrastructure/terraform/environments/staging
terraform plan -destroy
```

**Review what will be destroyed** (should show 66 resources)

### Step 4: Destroy Infrastructure

```bash
terraform destroy
```

**Confirmation prompts** - type `yes` to confirm

**What happens:**
1. Deletes ECR repositories and images
2. Deletes EKS node groups
3. Deletes EKS cluster
4. Deletes RDS database
5. Deletes ElastiCache cluster
6. Deletes security groups
7. Deletes NAT gateway
8. Deletes VPC and subnets
9. Deletes IAM roles
10. Deletes KMS keys

**Time**: 10-15 minutes

### Step 5: Verify Complete Deletion

```bash
# Should return empty or errors
aws eks list-clusters --region us-east-1
aws rds describe-db-instances --region us-east-1
aws elasticache describe-cache-clusters --region us-east-1
aws ec2 describe-vpcs --filters "Name=tag:Project,Values=Core" --region us-east-1
```

### Step 6: Clean Up Local State (Optional)

```bash
# Clear kubectl config
kubectl config delete-context arn:aws:eks:us-east-1:*:cluster/core-staging-eks

# Remove local Docker images
docker rmi core-backend core-frontend
docker rmi ${BACKEND_REPO}:latest ${BACKEND_REPO}:v1.0.0
docker rmi ${FRONTEND_REPO}:latest ${FRONTEND_REPO}:v1.0.0
```

---

## Post-Teardown State

### What Remains:
```
S3 Bucket (state):          $0.50/month
DynamoDB Table (locks):     $1.00/month
CloudWatch Logs (if any):   $0.50/month
Snapshots (if created):     $0.10/GB/month
────────────────────────────────────────
TOTAL:                      ~$2/month
```

### What's Deleted:
- ✅ All compute resources (EKS, nodes)
- ✅ All databases (RDS, ElastiCache)
- ✅ All networking (VPC, subnets, NAT)
- ✅ All storage (EBS volumes)
- ✅ All container images (ECR)

### Terraform State:
- ✅ Preserved in S3
- ✅ Can recreate identical infrastructure anytime
- ✅ All configuration saved

---

## Recreating the Environment

To redeploy later:

```bash
cd infrastructure/terraform/environments/staging
terraform apply
```

**Time**: 20-25 minutes  
**Result**: Identical infrastructure  
**Cost**: Starts fresh billing

---

## Cost Summary

| Phase | Time | Cost |
|-------|------|------|
| Deploy Infrastructure | 25 min | $0.18 |
| Configure & Deploy App | 20 min | $0.15 |
| Testing (2 hours) | 2 hours | $0.70 |
| Teardown | 15 min | $0.07 |
| **TOTAL** | **3 hours** | **$1.10** |
| After Teardown | Ongoing | $2/month |

**Compare to keeping running**: $178/month

**Savings**: $176/month (99%)

---

## Automation Option

### Create Helper Scripts

**deploy-staging.sh**:
```bash
#!/bin/bash
set -e

echo "🚀 Deploying staging infrastructure..."
cd infrastructure/terraform/environments/staging
terraform apply -auto-approve

echo "✅ Infrastructure deployed!"
echo "📝 Saving outputs..."
terraform output > ../../outputs.txt

echo "🔧 Configuring kubectl..."
aws eks update-kubeconfig --region us-east-1 --name core-staging-eks

echo "✨ Deployment complete!"
echo "Next steps:"
echo "  1. Build and push Docker images"
echo "  2. Deploy to Kubernetes"
echo "  3. Run tests"
```

**teardown-staging.sh**:
```bash
#!/bin/bash
set -e

echo "🗑️  Tearing down staging infrastructure..."

echo "1️⃣  Deleting Kubernetes resources..."
kubectl delete namespace core-staging --ignore-not-found=true

echo "2️⃣  Destroying Terraform infrastructure..."
cd infrastructure/terraform/environments/staging
terraform destroy -auto-approve

echo "✅ Teardown complete!"
echo "💰 Monthly cost reduced to ~$2 (state storage only)"
```

Make executable:
```bash
chmod +x deploy-staging.sh teardown-staging.sh
```

---

## Troubleshooting

### Terraform Destroy Hangs

**Problem**: EKS cluster won't delete

**Solution**:
```bash
# Manually delete node groups first
aws eks delete-nodegroup \
  --cluster-name core-staging-eks \
  --nodegroup-name core-staging-eks-node-group

# Wait for deletion
aws eks wait nodegroup-deleted \
  --cluster-name core-staging-eks \
  --nodegroup-name core-staging-eks-node-group

# Then retry destroy
terraform destroy
```

### ECR Repository Has Images

**Problem**: Can't delete ECR repo with images

**Solution**:
```bash
# Delete all images first
aws ecr batch-delete-image \
  --repository-name core-backend \
  --image-ids "$(aws ecr list-images --repository-name core-backend --query 'imageIds[*]' --output json)"

aws ecr batch-delete-image \
  --repository-name core-frontend \
  --image-ids "$(aws ecr list-images --repository-name core-frontend --query 'imageIds[*]' --output json)"
```

### VPC Dependencies Remain

**Problem**: VPC can't be deleted (dependencies)

**Solution**:
```bash
# Find remaining resources
aws ec2 describe-network-interfaces \
  --filters "Name=vpc-id,Values=<vpc-id>" \
  --query 'NetworkInterfaces[*].[NetworkInterfaceId,Description]'

# Delete ENIs manually
aws ec2 delete-network-interface --network-interface-id <eni-id>
```

---

## Best Practices

1. **Always test `terraform plan -destroy` first**
2. **Export logs/metrics before teardown**
3. **Create snapshots of databases if needed**
4. **Document any issues encountered**
5. **Verify complete deletion afterward**
6. **Keep Terraform state files backed up**

---

## Next Steps After Testing

Once you've validated the deployment process:

1. **For Ongoing Staging**: Keep infrastructure deployed
2. **For Cost Savings**: Use automated start/stop schedule
3. **For Production**: Adjust Terraform for production config
4. **For CD Pipeline**: Integrate with GitHub Actions

---

## Questions?

- **How long to recreate?** 20-25 minutes
- **Data preserved?** Only if snapshots created
- **Terraform state?** Preserved in S3
- **Cost to keep destroyed?** ~$2/month
- **Safe to destroy?** Yes, can recreate anytime

---

**Summary**: This workflow lets you test deployment for ~$1, validate everything works, then tear down completely. Perfect for dev/test cycles!

