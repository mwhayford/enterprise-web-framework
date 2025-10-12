# Copyright (c) Core. All Rights Reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

terraform {
  required_version = ">= 1.6.0"
  
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.23"
    }
  }

  backend "s3" {
    bucket         = "core-terraform-state-436399375303"
    key            = "environments/staging/terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "core-terraform-locks"
  }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Environment = "staging"
      Project     = "Core"
      ManagedBy   = "Terraform"
    }
  }
}

locals {
  name_prefix = "core-staging"
  common_tags = {
    Environment = "staging"
    Project     = "Core"
    ManagedBy   = "Terraform"
  }
}

# VPC Module
module "vpc" {
  source = "../../modules/vpc"

  name_prefix        = local.name_prefix
  vpc_cidr           = var.vpc_cidr
  cluster_name       = "${local.name_prefix}-eks"
  
  # Cost optimization for staging
  single_nat_gateway = true
  enable_flow_logs   = false

  tags = local.common_tags
}

# EKS Module
module "eks" {
  source = "../../modules/eks"

  cluster_name    = "${local.name_prefix}-eks"
  cluster_version = var.eks_cluster_version
  
  vpc_id             = module.vpc.vpc_id
  private_subnet_ids = module.vpc.private_subnet_ids
  public_subnet_ids  = module.vpc.public_subnet_ids

  # Staging configuration - cost optimized
  node_group_desired_size   = 2
  node_group_min_size       = 2
  node_group_max_size       = 5
  node_group_instance_types = ["t3.medium"]
  node_group_capacity_type  = "SPOT"  # Cost savings with spot instances
  node_group_disk_size      = 50

  # Enable logging for troubleshooting
  cluster_enabled_log_types = ["api", "audit", "authenticator", "controllerManager", "scheduler"]
  cloudwatch_log_group_retention_days = 7

  tags = local.common_tags
}

# RDS Module
module "rds" {
  source = "../../modules/rds"

  name_prefix = local.name_prefix
  vpc_id      = module.vpc.vpc_id
  subnet_ids  = module.vpc.database_subnet_ids
  
  allowed_security_groups = [module.eks.cluster_security_group_id]

  # Staging configuration - cost optimized
  engine_version    = "16.1"
  instance_class    = "db.t4g.micro"  # Smallest ARM-based instance (~$13/month)
  allocated_storage = 20
  max_allocated_storage = 50
  multi_az          = false  # Single AZ for staging
  storage_type      = "gp3"

  database_name   = var.database_name
  master_username = var.database_username

  backup_retention_period = 7
  backup_window          = "03:00-04:00"
  maintenance_window     = "sun:04:00-sun:05:00"

  # Disable for staging
  performance_insights_enabled = false
  monitoring_interval          = 0

  deletion_protection = false
  skip_final_snapshot = true

  tags = local.common_tags
}

# Redis Module
module "redis" {
  source = "../../modules/redis"

  name_prefix = local.name_prefix
  vpc_id      = module.vpc.vpc_id
  subnet_ids  = module.vpc.private_subnet_ids

  allowed_security_groups = [module.eks.cluster_security_group_id]

  # Staging configuration - cost optimized
  engine_version = "7.1"
  node_type      = "cache.t4g.micro"  # Smallest ARM-based instance (~$12/month)
  num_cache_nodes = 1  # Single node for staging

  # Disable high availability features for staging
  automatic_failover_enabled = false
  multi_az_enabled          = false
  auth_token_enabled        = false

  snapshot_retention_limit = 3
  log_retention_days       = 7

  tags = local.common_tags
}

# ECR Repositories
resource "aws_ecr_repository" "backend" {
  name                 = "core-backend"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  encryption_configuration {
    encryption_type = "AES256"
  }

  tags = local.common_tags
}

resource "aws_ecr_repository" "frontend" {
  name                 = "core-frontend"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  encryption_configuration {
    encryption_type = "AES256"
  }

  tags = local.common_tags
}

# ECR Lifecycle Policies
resource "aws_ecr_lifecycle_policy" "backend" {
  repository = aws_ecr_repository.backend.name

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Keep last 10 images"
      selection = {
        tagStatus     = "any"
        countType     = "imageCountMoreThan"
        countNumber   = 10
      }
      action = {
        type = "expire"
      }
    }]
  })
}

resource "aws_ecr_lifecycle_policy" "frontend" {
  repository = aws_ecr_repository.frontend.name

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Keep last 10 images"
      selection = {
        tagStatus     = "any"
        countType     = "imageCountMoreThan"
        countNumber   = 10
      }
      action = {
        type = "expire"
      }
    }]
  })
}

# Data source for current AWS account
data "aws_caller_identity" "current" {}

# Data source for AWS region
data "aws_region" "current" {}

