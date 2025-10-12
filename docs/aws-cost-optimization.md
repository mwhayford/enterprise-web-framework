# AWS Cost Optimization for Staging Environment

## Cost Comparison

### Original Configuration
```
EKS Control Plane:                $73/month
2x t3.medium nodes (On-Demand):   $60/month
RDS db.t4g.medium:                 $30/month
ElastiCache cache.t4g.small:      $15/month
NAT Gateway (Multi-AZ):            $64/month
ALB:                               $20/month
Data Transfer:                     ~$10/month
─────────────────────────────────────────────
TOTAL:                             $272/month
```

### Optimized Configuration
```
EKS Control Plane:                $73/month (fixed cost)
2x t3.medium nodes (SPOT):        $18/month (70% savings!)
RDS db.t4g.micro:                  $13/month (57% savings!)
ElastiCache cache.t4g.micro:      $12/month (20% savings!)
NAT Gateway (Single):              $32/month (50% savings!)
ALB:                               $20/month
Data Transfer:                     ~$10/month
─────────────────────────────────────────────
TOTAL:                             $178/month
SAVINGS:                           $94/month (35% reduction)
```

## Optimizations Applied

### 1. SPOT Instances for EKS Nodes (70% savings)
```hcl
node_group_capacity_type = "SPOT"
node_group_instance_types = ["t3.medium"]
```

**Benefits:**
- ✅ Up to 70% cost savings vs On-Demand
- ✅ Suitable for non-critical staging workloads
- ✅ EKS manages SPOT interruptions gracefully

**Considerations:**
- ⚠️ Nodes can be interrupted with 2-minute warning
- ⚠️ For production, use On-Demand or mix of SPOT + On-Demand

### 2. Smallest RDS Instance (db.t4g.micro)
```hcl
instance_class = "db.t4g.micro"
multi_az = false
```

**Specs:**
- 2 vCPUs (ARM-based Graviton2)
- 1GB RAM
- ~$13/month (vs $30 for db.t4g.medium)

**Benefits:**
- ✅ Sufficient for staging workloads
- ✅ ARM-based = better performance per dollar
- ✅ Can scale up easily if needed

### 3. Smallest Redis Instance (cache.t4g.micro)
```hcl
node_type = "cache.t4g.micro"
num_cache_nodes = 1
automatic_failover_enabled = false
multi_az_enabled = false
```

**Specs:**
- 2 vCPUs (ARM-based Graviton2)
- 0.5GB RAM
- ~$12/month (vs $15 for cache.t4g.small)

**Benefits:**
- ✅ Perfect for caching in staging
- ✅ Single AZ reduces complexity
- ✅ Can upgrade to multi-AZ for production

### 4. Single NAT Gateway
```hcl
single_nat_gateway = true
```

**Benefits:**
- ✅ 50% cost savings ($32 vs $64/month)
- ✅ Sufficient for staging traffic
- ✅ Eliminates cross-AZ data transfer charges

**Considerations:**
- ⚠️ Single point of failure for internet access
- ⚠️ All private subnets route through one NAT
- ⚠️ For production, use NAT per AZ

### 5. Disabled Non-Essential Features
```hcl
# RDS
performance_insights_enabled = false
monitoring_interval = 0
backup_retention_period = 7  # (vs 30 for production)

# VPC
enable_flow_logs = false

# CloudWatch
cloudwatch_log_group_retention_days = 7  # (vs 30+ for production)
```

**Benefits:**
- ✅ Reduced storage costs
- ✅ Lower CloudWatch costs
- ✅ Simplified monitoring stack

## Additional Cost Optimization Strategies

### A. Stop Non-Working Hours (Future)
Save ~65% by stopping resources when not in use:

```bash
# Stop staging environment (evenings/weekends)
# EKS nodes: Scale to 0
kubectl scale deployment --all --replicas=0 -n core-staging

# RDS: Create snapshot and delete (restore when needed)
aws rds create-db-snapshot --db-instance-identifier core-staging-db \
  --db-snapshot-identifier staging-snapshot-$(date +%Y%m%d)

# Estimated savings: ~$120/month (65% of $178)
```

### B. Use Reserved Instances (Future)
For 1-year commitment:

```
RDS Reserved Instance:     30% savings
ElastiCache Reserved:      30% savings
EC2 Reserved (for EKS):    30% savings

Potential additional savings: ~$30-40/month
```

### C. Right-Sizing (Monitor and Adjust)
After running for a month:

1. **Check CloudWatch Metrics**:
   - CPU utilization < 20% → downsize instances
   - Memory pressure → upsize RAM
   - Network throughput → adjust NAT/ALB

2. **RDS Performance Insights** (enable temporarily):
   - Query performance
   - Connection pool usage
   - Disk I/O patterns

3. **EKS Node Metrics**:
   - Pod resource requests vs actual usage
   - Node CPU/memory utilization
   - Spot interruption frequency

### D. Storage Optimization
```hcl
# RDS - Use gp3 instead of gp2
storage_type = "gp3"  # Same cost, better performance

# ECR - Lifecycle policy
# Keep only last 10 images (already configured)

# EKS - Use gp3 EBS volumes
node_group_disk_size = 50  # Minimum needed, expands if required
```

## Cost Monitoring Setup

### 1. Enable AWS Cost Explorer
```bash
# Tag all resources for cost allocation
default_tags {
  tags = {
    Environment = "staging"
    Project     = "Core"
    ManagedBy   = "Terraform"
  }
}
```

### 2. Set Up Budget Alerts
```bash
# Alert when staging costs exceed $200/month
aws budgets create-budget \
  --account-id 436399375303 \
  --budget file://budget-staging.json
```

### 3. Daily Cost Report (Future Enhancement)
- CloudWatch dashboard with daily spend
- Lambda function to send Slack notifications
- Weekly cost analysis email

## Estimated Monthly Breakdown

| Service | Type | Quantity | Unit Cost | Monthly Cost |
|---------|------|----------|-----------|--------------|
| EKS Control Plane | Fixed | 1 cluster | $0.10/hour | $73.00 |
| EC2 (SPOT) | Variable | 2x t3.medium | ~$0.0125/hour | $18.00 |
| RDS | Database | 1x db.t4g.micro | $0.018/hour | $13.00 |
| ElastiCache | Cache | 1x cache.t4g.micro | $0.017/hour | $12.00 |
| NAT Gateway | Network | 1 gateway | $0.045/hour | $32.00 |
| ALB | Load Balancer | 1 ALB | $0.0225/hour | $16.00 |
| ALB LCUs | Traffic | ~10 LCUs/hour | $0.008/LCU | $6.00 |
| Data Transfer | Network | ~50GB/month | $0.09/GB | $4.50 |
| S3 (State) | Storage | <1GB | Minimal | $0.50 |
| DynamoDB (Locks) | Database | Pay per request | Minimal | $1.00 |
| CloudWatch Logs | Monitoring | ~5GB/month | $0.50/GB | $2.50 |
| **TOTAL** | | | | **$178.50/month** |

## Production Cost Estimate

For production with high availability:

```
EKS Control Plane:                $73/month
4x t3.medium nodes (On-Demand):   $120/month (2 per AZ)
RDS db.t4g.large (Multi-AZ):      $120/month
ElastiCache cache.r7g.large:      $180/month (cluster mode)
NAT Gateway (3x for Multi-AZ):    $96/month
ALB:                               $30/month
CloudFront CDN:                    $20/month
Route53:                           $10/month
ACM (SSL):                         $0/month (free)
Data Transfer:                     $50/month
Backups & Snapshots:               $20/month
─────────────────────────────────────────────
TOTAL:                             $719/month

With Reserved Instances (1-year):  $550/month (23% savings)
With 3-year commitment:            $450/month (37% savings)
```

## Recommendations

### Immediate (Today)
1. ✅ Deploy with optimized staging configuration
2. ✅ Monitor costs in AWS Cost Explorer
3. ✅ Set budget alert at $200/month

### Week 1
1. Monitor CloudWatch metrics for right-sizing
2. Check SPOT interruption frequency
3. Review actual vs estimated costs

### Month 1
1. Analyze usage patterns
2. Adjust instance sizes if needed
3. Consider Reserved Instances if keeping long-term

### Before Production
1. Enable Multi-AZ for RDS and ElastiCache
2. Switch critical EKS nodes to On-Demand
3. Add NAT Gateway per AZ
4. Enable all monitoring features
5. Increase backup retention periods

## Tools for Cost Management

1. **AWS Cost Explorer**: Daily cost tracking
2. **AWS Budgets**: Alert when exceeding thresholds
3. **AWS Trusted Advisor**: Cost optimization recommendations
4. **Kubecost** (for Kubernetes): Per-pod cost allocation
5. **Infracost** (for Terraform): Cost estimation before apply

## Conclusion

**Optimized Staging Cost**: ~$178/month (35% savings from baseline)

**Cost Breakdown**:
- 41% - EKS Control Plane (fixed)
- 18% - NAT Gateway (network access)
- 10% - SPOT Instances (compute)
- 7% - RDS (database)
- 7% - ElastiCache (cache)
- 9% - ALB (load balancing)
- 8% - Misc (data transfer, logs, etc.)

**Next Optimization Opportunities**:
- Stop environment during off-hours: Save $120/month
- Reserved Instances (1-year): Save $30-40/month
- Further right-sizing after monitoring: Save $10-20/month

**Total Potential Savings**: Up to 65% with aggressive optimization

