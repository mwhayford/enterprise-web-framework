# Copyright (c) Core. All Rights Reserved.
# Licensed under the MIT License. See LICENSE in the project root for license information.

variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "vpc_cidr" {
  description = "CIDR block for VPC"
  type        = string
  default     = "10.1.0.0/16"
}

variable "availability_zones" {
  description = "List of availability zones"
  type        = list(string)
  default     = ["us-east-1a", "us-east-1b", "us-east-1c"]
}

variable "eks_cluster_version" {
  description = "Kubernetes version for EKS cluster"
  type        = string
  default     = "1.28"
}

variable "database_name" {
  description = "Name of the default database"
  type        = string
  default     = "CoreDb"
}

variable "database_username" {
  description = "Master username for database"
  type        = string
  default     = "postgres"
}

variable "alarm_sns_topic_arn" {
  description = "SNS topic ARN for CloudWatch alarms"
  type        = string
  default     = ""
}

