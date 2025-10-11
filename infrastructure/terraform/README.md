# Terraform Infrastructure as Code

## Overview

This directory contains Terraform configurations for provisioning cloud-agnostic infrastructure for the Core application.

## Structure

```
infrastructure/terraform/
├── modules/
│   ├── vpc/          # VPC, subnets, NAT, IGW
│   ├── eks/          # EKS cluster and node groups
│   ├── rds/          # PostgreSQL RDS database
│   └── redis/        # ElastiCache Redis cluster
├── environments/
│   ├── staging/      # Staging environment configuration
│   └── production/   # Production environment configuration
└── README.md         # This file
```

## Prerequisites

1. **Terraform** >= 1.6.0
2. **AWS CLI** configured with appropriate credentials
3. **kubectl** for Kubernetes management
4. **AWS Account** with necessary permissions

## Module Overview

### VPC Module

Creates a highly available VPC with:
- Public subnets (3 AZs)
- Private subnets (3 AZs)
- Database subnets (3 AZs)
- Internet Gateway
- NAT Gateways (configurable: single or per-AZ)
- VPC Flow Logs (optional)

### EKS Module

Provisions Amazon EKS with:
- EKS Control Plane (managed Kubernetes)
- Node Groups (ON_DEMAND or SPOT)
- IRSA (IAM Roles for Service Accounts)
- Add-ons: VPC CNI, CoreDNS, kube-proxy, EBS CSI Driver
- CloudWatch logging
- KMS encryption for secrets

### RDS Module

Creates PostgreSQL RDS with:
- PostgreSQL 16
- Multi-AZ deployment (production)
- Automated backups
- KMS encryption
- Enhanced monitoring
- CloudWatch logs
- Secrets Manager integration

### Redis Module

Sets up ElastiCache Redis with:
- Redis 7.x
- Multi-AZ replication
- Automatic failover
- KMS encryption
- CloudWatch metrics

## Quick Start

### 1. Initialize Terraform

```bash
cd infrastructure/terraform/environments/staging
terraform init
```

### 2. Plan Infrastructure

```bash
terraform plan -out=tfplan
```

### 3. Apply Infrastructure

```bash
terraform apply tfplan
```

### 4. Configure kubectl

```bash
aws eks update-kubeconfig \
  --name core-staging-eks \
  --region us-east-1
```

### 5. Verify Cluster

```bash
kubectl get nodes
kubectl get pods --all-namespaces
```

## Environment Configuration

### Staging Environment

**File**: `environments/staging/main.tf`

```hcl
module "vpc" {
  source = "../../modules/vpc"
  
  name_prefix         = "core-staging"
  vpc_cidr            = "10.0.0.0/16"
  cluster_name        = "core-staging-eks"
  single_nat_gateway  = true  # Cost savings
  enable_flow_logs    = false
  
  tags = local.common_tags
}

module "eks" {
  source = "../../modules/eks"
  
  cluster_name              = "core-staging-eks"
  cluster_version           = "1.28"
  vpc_id                    = module.vpc.vpc_id
  private_subnet_ids        = module.vpc.private_subnet_ids
  public_subnet_ids         = module.vpc.public_subnet_ids
  
  node_group_desired_size   = 2
  node_group_min_size       = 2
  node_group_max_size       = 5
  node_group_instance_types = ["t3.medium"]
  node_group_capacity_type  = "SPOT"  # Cost savings
  
  tags = local.common_tags
}

module "rds" {
  source = "../../modules/rds"
  
  name_prefix       = "core-staging"
  vpc_id            = module.vpc.vpc_id
  subnet_ids        = module.vpc.database_subnet_ids
  allowed_security_groups = [module.eks.cluster_security_group_id]
  
  instance_class    = "db.t4g.medium"
  allocated_storage = 20
  multi_az          = false  # Single AZ for staging
  
  deletion_protection = false
  skip_final_snapshot = true
  
  tags = local.common_tags
}

module "redis" {
  source = "../../modules/redis"
  
  name_prefix       = "core-staging"
  vpc_id            = module.vpc.vpc_id
  subnet_ids        = module.vpc.private_subnet_ids
  allowed_security_groups = [module.eks.cluster_security_group_id]
  
  node_type         = "cache.t4g.small"
  num_cache_nodes   = 1  # Single node for staging
  
  tags = local.common_tags
}
```

### Production Environment

**File**: `environments/production/main.tf`

```hcl
module "vpc" {
  source = "../../modules/vpc"
  
  name_prefix         = "core-production"
  vpc_cidr            = "10.1.0.0/16"
  cluster_name        = "core-production-eks"
  single_nat_gateway  = false  # High availability
  enable_flow_logs    = true
  
  tags = local.common_tags
}

module "eks" {
  source = "../../modules/eks"
  
  cluster_name              = "core-production-eks"
  cluster_version           = "1.28"
  vpc_id                    = module.vpc.vpc_id
  private_subnet_ids        = module.vpc.private_subnet_ids
  public_subnet_ids         = module.vpc.public_subnet_ids
  
  node_group_desired_size   = 5
  node_group_min_size       = 5
  node_group_max_size       = 20
  node_group_instance_types = ["t3.large", "t3.xlarge"]
  node_group_capacity_type  = "ON_DEMAND"  # Reliability
  
  tags = local.common_tags
}

module "rds" {
  source = "../../modules/rds"
  
  name_prefix       = "core-production"
  vpc_id            = module.vpc.vpc_id
  subnet_ids        = module.vpc.database_subnet_ids
  allowed_security_groups = [module.eks.cluster_security_group_id]
  
  instance_class    = "db.r6g.xlarge"
  allocated_storage = 100
  max_allocated_storage = 500
  multi_az          = true  # High availability
  
  backup_retention_period = 30
  deletion_protection     = true
  skip_final_snapshot     = false
  
  performance_insights_enabled = true
  monitoring_interval          = 60
  
  tags = local.common_tags
}

module "redis" {
  source = "../../modules/redis"
  
  name_prefix       = "core-production"
  vpc_id            = module.vpc.vpc_id
  subnet_ids        = module.vpc.private_subnet_ids
  allowed_security_groups = [module.eks.cluster_security_group_id]
  
  node_type         = "cache.r6g.large"
  num_cache_nodes   = 3  # Multi-AZ with replicas
  automatic_failover_enabled = true
  
  tags = local.common_tags
}
```

## State Management

### Remote State (Recommended)

Store Terraform state in S3 with DynamoDB locking:

```hcl
terraform {
  backend "s3" {
    bucket         = "core-terraform-state"
    key            = "environments/staging/terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "core-terraform-locks"
  }
}
```

### Create State Backend

```bash
# Create S3 bucket for state
aws s3 mb s3://core-terraform-state --region us-east-1

# Enable versioning
aws s3api put-bucket-versioning \
  --bucket core-terraform-state \
  --versioning-configuration Status=Enabled

# Create DynamoDB table for locks
aws dynamodb create-table \
  --table-name core-terraform-locks \
  --attribute-definitions AttributeName=LockID,AttributeType=S \
  --key-schema AttributeName=LockID,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST \
  --region us-east-1
```

## Cost Optimization

### Staging Environment
- Single NAT Gateway: $32/month savings
- SPOT instances: ~70% cost reduction
- Single-AZ RDS: 50% cost reduction
- Smaller instance types
- **Estimated**: ~$200-300/month

### Production Environment
- Multi-AZ NAT Gateways: High availability
- ON_DEMAND instances: Reliability
- Multi-AZ RDS: High availability
- Larger instance types for performance
- **Estimated**: ~$1000-1500/month

## Security Best Practices

1. **Network Isolation**
   - Private subnets for EKS nodes
   - Database subnets isolated
   - Security groups with minimal access

2. **Encryption**
   - KMS encryption for EKS secrets
   - KMS encryption for RDS
   - KMS encryption for Redis
   - TLS in transit

3. **IAM**
   - IRSA for pod-level permissions
   - Least privilege policies
   - No hard-coded credentials

4. **Monitoring**
   - VPC Flow Logs
   - EKS Control Plane logs
   - RDS Enhanced Monitoring
   - CloudWatch metrics and alarms

## Disaster Recovery

### Backup Strategy

**RDS**:
- Automated daily backups
- 30-day retention (production)
- Point-in-time recovery
- Cross-region snapshots (optional)

**Application Data**:
- EBS volume snapshots
- S3 versioning for objects
- Velero for Kubernetes backups

### Recovery Procedures

1. **Database Recovery**:
   ```bash
   aws rds restore-db-instance-from-db-snapshot \
     --db-instance-identifier core-prod-restored \
     --db-snapshot-identifier core-prod-snapshot
   ```

2. **Cluster Recovery**:
   ```bash
   terraform apply -target=module.eks
   kubectl apply -k infrastructure/kubernetes/overlays/production
   ```

## Maintenance

### Upgrading EKS

```bash
# Update cluster version in main.tf
cluster_version = "1.29"

# Plan and apply
terraform plan
terraform apply

# Upgrade node groups
kubectl drain <node-name> --ignore-daemonsets
terraform apply
```

### Scaling

```bash
# Update desired size in main.tf
node_group_desired_size = 10

# Apply changes
terraform apply
```

### Database Maintenance

```bash
# Modify maintenance window
maintenance_window = "sun:03:00-sun:04:00"

terraform apply
```

## Troubleshooting

### Terraform State Locked

```bash
# List locks
aws dynamodb scan --table-name core-terraform-locks

# Force unlock (if needed)
terraform force-unlock <lock-id>
```

### EKS Node Issues

```bash
# Check node status
kubectl get nodes
kubectl describe node <node-name>

# Recycle nodes
terraform taint module.eks.aws_eks_node_group.main
terraform apply
```

### RDS Connection Issues

```bash
# Test connection from EKS pod
kubectl run -it --rm debug --image=postgres:16 --restart=Never -- \
  psql -h <rds-endpoint> -U postgres -d CoreDb
```

## Cleanup

### Destroy Staging

```bash
cd environments/staging
terraform destroy
```

### Destroy Production

```bash
cd environments/production

# Ensure you mean it!
echo "DESTROY_PRODUCTION" | terraform destroy
```

## Additional Resources

- [Terraform AWS Provider](https://registry.terraform.io/providers/hashicorp/aws/latest/docs)
- [EKS Best Practices](https://aws.github.io/aws-eks-best-practices/)
- [RDS Best Practices](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/CHAP_BestPractices.html)
- [VPC Best Practices](https://docs.aws.amazon.com/vpc/latest/userguide/vpc-security-best-practices.html)

## Support

For issues or questions:
- Check Terraform docs
- Review AWS service limits
- Check CloudWatch logs
- Create GitHub issue

