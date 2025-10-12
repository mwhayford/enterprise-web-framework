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
    key            = "environments/production/terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "core-terraform-locks"
  }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Environment = "production"
      Project     = "Core"
      ManagedBy   = "Terraform"
    }
  }
}

locals {
  name_prefix = "core-production"
  common_tags = {
    Environment = "production"
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
  
  # High availability for production
  single_nat_gateway = false  # NAT Gateway per AZ
  enable_flow_logs   = true   # Enable VPC flow logs

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

  # Production configuration - high availability and reliability
  node_group_desired_size   = 5
  node_group_min_size       = 5
  node_group_max_size       = 20
  node_group_instance_types = ["t3.large", "t3.xlarge"]
  node_group_capacity_type  = "ON_DEMAND"  # Reliability over cost
  node_group_disk_size      = 100

  # Comprehensive logging
  cluster_enabled_log_types = ["api", "audit", "authenticator", "controllerManager", "scheduler"]
  cloudwatch_log_group_retention_days = 30

  tags = local.common_tags
}

# RDS Module
module "rds" {
  source = "../../modules/rds"

  name_prefix = local.name_prefix
  vpc_id      = module.vpc.vpc_id
  subnet_ids  = module.vpc.database_subnet_ids
  
  allowed_security_groups = [module.eks.cluster_security_group_id]

  # Production configuration - high availability
  engine_version    = "16.1"
  instance_class    = "db.r6g.xlarge"
  allocated_storage = 100
  max_allocated_storage = 500
  multi_az          = true  # High availability
  storage_type      = "gp3"

  database_name   = var.database_name
  master_username = var.database_username

  backup_retention_period = 30
  backup_window          = "03:00-04:00"
  maintenance_window     = "sun:04:00-sun:05:00"

  # Enable for production
  performance_insights_enabled = true
  monitoring_interval          = 60  # Enhanced monitoring

  deletion_protection = true   # Protect from accidental deletion
  skip_final_snapshot = false  # Always take final snapshot

  tags = local.common_tags
}

# Redis Module
module "redis" {
  source = "../../modules/redis"

  name_prefix = local.name_prefix
  vpc_id      = module.vpc.vpc_id
  subnet_ids  = module.vpc.private_subnet_ids

  allowed_security_groups = [module.eks.cluster_security_group_id]

  # Production configuration - high availability
  engine_version = "7.1"
  node_type      = "cache.r6g.large"
  num_cache_nodes = 3  # Multi-node for high availability

  # Enable high availability features
  automatic_failover_enabled = true
  multi_az_enabled          = true
  auth_token_enabled        = true

  snapshot_retention_limit = 7
  log_retention_days       = 30

  tags = local.common_tags
}

# ECR Repositories
resource "aws_ecr_repository" "backend" {
  name                 = "core-backend"
  image_tag_mutability = "IMMUTABLE"  # Immutable for production

  image_scanning_configuration {
    scan_on_push = true
  }

  encryption_configuration {
    encryption_type = "KMS"
    kms_key         = aws_kms_key.ecr.arn
  }

  tags = local.common_tags
}

resource "aws_ecr_repository" "frontend" {
  name                 = "core-frontend"
  image_tag_mutability = "IMMUTABLE"  # Immutable for production

  image_scanning_configuration {
    scan_on_push = true
  }

  encryption_configuration {
    encryption_type = "KMS"
    kms_key         = aws_kms_key.ecr.arn
  }

  tags = local.common_tags
}

# KMS Key for ECR
resource "aws_kms_key" "ecr" {
  description             = "KMS key for ECR encryption"
  deletion_window_in_days = 7
  enable_key_rotation     = true

  tags = merge(
    local.common_tags,
    {
      Name = "${local.name_prefix}-ecr-key"
    }
  )
}

# ECR Lifecycle Policies
resource "aws_ecr_lifecycle_policy" "backend" {
  repository = aws_ecr_repository.backend.name

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Keep last 30 images"
      selection = {
        tagStatus     = "any"
        countType     = "imageCountMoreThan"
        countNumber   = 30
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
      description  = "Keep last 30 images"
      selection = {
        tagStatus     = "any"
        countType     = "imageCountMoreThan"
        countNumber   = 30
      }
      action = {
        type = "expire"
      }
    }]
  })
}

# CloudWatch Alarms for critical metrics
resource "aws_cloudwatch_metric_alarm" "rds_cpu" {
  alarm_name          = "${local.name_prefix}-rds-cpu-utilization"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "CPUUtilization"
  namespace           = "AWS/RDS"
  period              = "300"
  statistic           = "Average"
  threshold           = "80"
  alarm_description   = "RDS CPU utilization is too high"
  alarm_actions       = var.alarm_sns_topic_arn != "" ? [var.alarm_sns_topic_arn] : []

  dimensions = {
    DBInstanceIdentifier = module.rds.db_instance_id
  }

  tags = local.common_tags
}

resource "aws_cloudwatch_metric_alarm" "rds_storage" {
  alarm_name          = "${local.name_prefix}-rds-free-storage"
  comparison_operator = "LessThanThreshold"
  evaluation_periods  = "1"
  metric_name         = "FreeStorageSpace"
  namespace           = "AWS/RDS"
  period              = "300"
  statistic           = "Average"
  threshold           = "10737418240"  # 10 GB in bytes
  alarm_description   = "RDS free storage space is low"
  alarm_actions       = var.alarm_sns_topic_arn != "" ? [var.alarm_sns_topic_arn] : []

  dimensions = {
    DBInstanceIdentifier = module.rds.db_instance_id
  }

  tags = local.common_tags
}

# Data source for current AWS account
data "aws_caller_identity" "current" {}

# Data source for AWS region
data "aws_region" "current" {}

