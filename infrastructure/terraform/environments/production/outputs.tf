# Copyright (c) Core. All Rights Reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

# VPC Outputs
output "vpc_id" {
  description = "VPC ID"
  value       = module.vpc.vpc_id
}

output "private_subnet_ids" {
  description = "Private subnet IDs"
  value       = module.vpc.private_subnet_ids
}

output "public_subnet_ids" {
  description = "Public subnet IDs"
  value       = module.vpc.public_subnet_ids
}

# EKS Outputs
output "eks_cluster_name" {
  description = "EKS cluster name"
  value       = module.eks.cluster_name
}

output "eks_cluster_endpoint" {
  description = "EKS cluster endpoint"
  value       = module.eks.cluster_endpoint
}

output "eks_cluster_security_group_id" {
  description = "EKS cluster security group ID"
  value       = module.eks.cluster_security_group_id
}

output "eks_cluster_oidc_issuer_url" {
  description = "OIDC issuer URL for EKS cluster"
  value       = module.eks.oidc_provider_url
}

# RDS Outputs
output "rds_endpoint" {
  description = "RDS instance endpoint"
  value       = module.rds.db_instance_endpoint
}

output "rds_address" {
  description = "RDS instance address"
  value       = module.rds.db_instance_address
}

output "rds_port" {
  description = "RDS instance port"
  value       = module.rds.db_instance_port
}

output "rds_database_name" {
  description = "RDS database name"
  value       = module.rds.db_name
}

output "rds_secret_arn" {
  description = "ARN of Secrets Manager secret containing RDS credentials"
  value       = module.rds.secret_arn
}

# Redis Outputs
output "redis_endpoint" {
  description = "Redis primary endpoint"
  value       = module.redis.primary_endpoint_address
}

output "redis_configuration_endpoint" {
  description = "Redis configuration endpoint"
  value       = module.redis.configuration_endpoint_address
}

output "redis_port" {
  description = "Redis port"
  value       = module.redis.port
}

# ECR Outputs
output "ecr_backend_repository_url" {
  description = "ECR repository URL for backend"
  value       = aws_ecr_repository.backend.repository_url
}

output "ecr_frontend_repository_url" {
  description = "ECR repository URL for frontend"
  value       = aws_ecr_repository.frontend.repository_url
}

# Connection String Output (for convenience)
output "database_connection_string" {
  description = "Database connection string (without password)"
  value       = "Host=${module.rds.db_instance_address};Port=${module.rds.db_instance_port};Database=${module.rds.db_name};Username=${module.rds.master_username};Password=<see_secrets_manager>"
  sensitive   = true
}

output "redis_connection_string" {
  description = "Redis connection string"
  value       = "${module.redis.primary_endpoint_address}:${module.redis.port}"
}

# AWS Account Info
output "aws_account_id" {
  description = "AWS Account ID"
  value       = data.aws_caller_identity.current.account_id
}

output "aws_region" {
  description = "AWS Region"
  value       = data.aws_region.current.name
}

# KMS Keys
output "ecr_kms_key_id" {
  description = "KMS key ID for ECR encryption"
  value       = aws_kms_key.ecr.id
}

