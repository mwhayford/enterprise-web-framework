# Kubernetes Deployment Configuration

## Overview

This directory contains Kubernetes manifests for deploying the Core application to Amazon EKS using Kustomize for environment-specific overlays.

## Structure

```
infrastructure/kubernetes/
├── base/                          # Base Kubernetes resources
│   ├── namespace.yaml             # Namespace definition
│   ├── configmap.yaml             # Application configuration
│   ├── secrets.yaml               # Sensitive configuration (encrypted)
│   ├── backend-deployment.yaml    # Backend API deployment
│   ├── backend-service.yaml       # Backend service
│   ├── frontend-deployment.yaml   # Frontend deployment
│   ├── frontend-service.yaml      # Frontend service
│   ├── ingress.yaml               # AWS ALB Ingress
│   ├── hpa.yaml                   # Horizontal Pod Autoscaler
│   └── kustomization.yaml         # Base kustomization
├── overlays/
│   ├── staging/                   # Staging environment
│   │   ├── kustomization.yaml
│   │   ├── deployment-patch.yaml
│   │   └── configmap-patch.yaml
│   └── production/                # Production environment
│       ├── kustomization.yaml
│       ├── deployment-patch.yaml
│       └── hpa-patch.yaml
└── README.md                      # This file
```

## Prerequisites

1. **AWS CLI** configured with appropriate credentials
2. **kubectl** (v1.28+)
3. **kustomize** (v5.0+)
4. **EKS cluster** provisioned (see Terraform configuration)
5. **AWS Load Balancer Controller** installed in the cluster
6. **Metrics Server** installed for HPA

## Quick Start

### 1. Setup kubectl Context

```bash
aws eks update-kubeconfig \
  --name your-eks-cluster-name \
  --region us-east-1
```

### 2. Verify Cluster Access

```bash
kubectl cluster-info
kubectl get nodes
```

### 3. Deploy to Staging

```bash
# Review manifests
kubectl kustomize overlays/staging

# Apply to cluster
kubectl apply -k overlays/staging

# Watch deployment progress
kubectl rollout status deployment/core-backend-staging -n core-staging
kubectl rollout status deployment/core-frontend-staging -n core-staging
```

### 4. Deploy to Production

```bash
# Review manifests
kubectl kustomize overlays/production

# Apply to cluster
kubectl apply -k overlays/production

# Watch deployment progress
kubectl rollout status deployment/core-backend-production -n core-production
kubectl rollout status deployment/core-frontend-production -n core-production
```

## Configuration

### Secrets Management

**⚠️ Important**: The `secrets.yaml` file contains placeholder values. Replace them with actual secrets before deploying.

#### Using AWS Secrets Manager

Recommended approach for EKS:

```bash
# Store secrets in AWS Secrets Manager
aws secretsmanager create-secret \
  --name core/database-connection \
  --secret-string "Host=..."

# Use External Secrets Operator to sync to Kubernetes
kubectl apply -f external-secrets/
```

#### Using kubectl

```bash
# Create secret from command line
kubectl create secret generic core-secrets \
  --from-literal=ConnectionStrings__DefaultConnection="Host=..." \
  --namespace core-staging
```

### ConfigMap Updates

To update configuration:

```bash
# Edit configmap
kubectl edit configmap core-config -n core-staging

# Restart pods to pick up changes
kubectl rollout restart deployment/core-backend -n core-staging
```

## Deployment Strategy

### Rolling Update

Default strategy with zero downtime:

```yaml
strategy:
  type: RollingUpdate
  rollingUpdate:
    maxSurge: 1        # Add 1 extra pod during update
    maxUnavailable: 0  # Keep all current pods running
```

### Blue-Green Deployment

For major updates:

```bash
# Deploy new version alongside old
kubectl apply -k overlays/production-blue

# Test new version
kubectl port-forward svc/core-backend-blue 8080:80 -n core-production

# Switch traffic (update ingress)
kubectl patch ingress core-ingress \
  --type=json \
  -p='[{"op": "replace", "path": "/spec/rules/0/http/paths/0/backend/service/name", "value":"core-backend-blue"}]'

# Verify and delete old version
kubectl delete -k overlays/production-green
```

## Horizontal Pod Autoscaling

### Backend HPA

```yaml
minReplicas: 3 (staging: 2, production: 5)
maxReplicas: 10 (production: 20)
Metrics:
  - CPU: 70%
  - Memory: 80%
```

### Monitoring HPA

```bash
# Watch HPA status
kubectl get hpa -n core-staging --watch

# Describe HPA for details
kubectl describe hpa core-backend-hpa -n core-staging
```

## Networking

### Ingress (AWS ALB)

The application uses AWS Application Load Balancer (ALB) Ingress Controller:

```yaml
Hosts:
  - api.example.com  → core-backend
  - app.example.com  → core-frontend

Annotations:
  - HTTPS redirect
  - SSL certificate (ACM)
  - Health checks
  - Cross-zone load balancing
```

### Service Mesh (Optional)

For advanced traffic management, consider:
- **Istio**: Service mesh with mTLS
- **Linkerd**: Lightweight service mesh
- **AWS App Mesh**: Native AWS solution

## Resource Limits

### Staging Environment

**Backend**:
- Requests: 250m CPU, 256Mi memory
- Limits: 1000m CPU, 1Gi memory
- Replicas: 2-10

**Frontend**:
- Requests: 50m CPU, 64Mi memory
- Limits: 250m CPU, 256Mi memory
- Replicas: 2-10

### Production Environment

**Backend**:
- Requests: 1000m CPU, 1Gi memory
- Limits: 4000m CPU, 4Gi memory
- Replicas: 5-20

**Frontend**:
- Requests: 200m CPU, 256Mi memory
- Limits: 1000m CPU, 1Gi memory
- Replicas: 5-20

## Monitoring and Logging

### Prometheus Metrics

Pods are annotated for Prometheus scraping:

```yaml
annotations:
  prometheus.io/scrape: "true"
  prometheus.io/port: "80"
  prometheus.io/path: "/metrics"
```

### Logs

```bash
# View backend logs
kubectl logs -f deployment/core-backend -n core-staging

# View logs from all pods
kubectl logs -f -l app=core-backend -n core-staging --max-log-requests=10

# Stream logs to CloudWatch
# (Configured via Fluent Bit DaemonSet)
```

### Health Checks

**Liveness Probe**: Restarts unhealthy pods
```yaml
httpGet:
  path: /health
  port: 80
initialDelaySeconds: 30
periodSeconds: 10
```

**Readiness Probe**: Removes unhealthy pods from service
```yaml
httpGet:
  path: /health
  port: 80
initialDelaySeconds: 10
periodSeconds: 5
```

**Startup Probe**: Handles slow-starting containers
```yaml
httpGet:
  path: /health
  port: 80
failureThreshold: 30
periodSeconds: 10
```

## Database Migrations

### Automatic Migrations

Migrations run automatically during deployment via init container:

```yaml
initContainers:
- name: migrations
  image: core-backend:latest
  command: ["dotnet", "ef", "database", "update"]
```

### Manual Migrations

```bash
# Run migrations job
kubectl apply -f migrations-job.yaml

# Check job status
kubectl get jobs -n core-staging
kubectl logs job/migrations -n core-staging
```

## Troubleshooting

### Pod Not Starting

```bash
# Check pod status
kubectl get pods -n core-staging

# Describe pod for events
kubectl describe pod <pod-name> -n core-staging

# Check logs
kubectl logs <pod-name> -n core-staging

# Get previous logs (if crashed)
kubectl logs <pod-name> -n core-staging --previous
```

### Image Pull Errors

```bash
# Verify ECR access
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin \
  <account-id>.dkr.ecr.us-east-1.amazonaws.com

# Check service account IAM role
kubectl describe sa core-backend-sa -n core-staging
```

### Ingress Not Working

```bash
# Check ingress status
kubectl get ingress -n core-staging
kubectl describe ingress core-ingress -n core-staging

# Verify ALB controller logs
kubectl logs -n kube-system \
  -l app.kubernetes.io/name=aws-load-balancer-controller
```

### HPA Not Scaling

```bash
# Check metrics server
kubectl get deployment metrics-server -n kube-system

# Verify metrics available
kubectl top pods -n core-staging

# Check HPA events
kubectl describe hpa core-backend-hpa -n core-staging
```

### Database Connection Issues

```bash
# Test database connectivity from pod
kubectl exec -it <pod-name> -n core-staging -- /bin/bash
# Inside pod:
apt-get update && apt-get install -y postgresql-client
psql -h <rds-endpoint> -U postgres -d CoreDb

# Check security groups
aws ec2 describe-security-groups --group-ids <sg-id>
```

## Rollback

### Rollback Deployment

```bash
# View rollout history
kubectl rollout history deployment/core-backend -n core-staging

# Rollback to previous version
kubectl rollout undo deployment/core-backend -n core-staging

# Rollback to specific revision
kubectl rollout undo deployment/core-backend \
  --to-revision=2 \
  -n core-staging

# Watch rollback progress
kubectl rollout status deployment/core-backend -n core-staging
```

## Cleanup

### Delete Staging Environment

```bash
kubectl delete -k overlays/staging
```

### Delete Production Environment

```bash
kubectl delete -k overlays/production
```

### Delete All Resources

```bash
kubectl delete namespace core-staging
kubectl delete namespace core-production
```

## Best Practices

1. **Always use Kustomize**: Don't apply base manifests directly
2. **Test in staging first**: Always deploy to staging before production
3. **Use semantic versioning**: Tag Docker images with version numbers
4. **Monitor resource usage**: Adjust requests/limits based on actual usage
5. **Enable Pod Disruption Budgets**: Protect critical workloads during updates
6. **Use Network Policies**: Restrict pod-to-pod communication
7. **Implement Resource Quotas**: Prevent resource exhaustion
8. **Regular backups**: Backup critical data and configurations
9. **Security scanning**: Scan images for vulnerabilities
10. **Documentation**: Keep this README updated with changes

## Security Considerations

### RBAC

Implement Role-Based Access Control:

```bash
# Create role for developers
kubectl create role developer \
  --verb=get,list,watch \
  --resource=pods,services \
  -n core-staging

# Bind role to user
kubectl create rolebinding developer-binding \
  --role=developer \
  --user=developer@example.com \
  -n core-staging
```

### Network Policies

Restrict network traffic:

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: backend-network-policy
spec:
  podSelector:
    matchLabels:
      app: core-backend
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: core-frontend
```

### Pod Security Standards

Enforce pod security:

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: core-production
  labels:
    pod-security.kubernetes.io/enforce: restricted
    pod-security.kubernetes.io/audit: restricted
    pod-security.kubernetes.io/warn: restricted
```

## Resources

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Kustomize Documentation](https://kustomize.io/)
- [AWS EKS Best Practices](https://aws.github.io/aws-eks-best-practices/)
- [AWS Load Balancer Controller](https://kubernetes-sigs.github.io/aws-load-balancer-controller/)
- [Horizontal Pod Autoscaler](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/)

