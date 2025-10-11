# Deployment Documentation

## Overview

This document provides comprehensive deployment instructions for the Enterprise Web Application Framework across different environments, from local development to production deployment on AWS EKS.

## Deployment Architecture

### Production Environment
- **Kubernetes Cluster**: AWS EKS
- **Database**: AWS RDS PostgreSQL
- **Load Balancer**: AWS Application Load Balancer
- **CDN**: AWS CloudFront
- **Container Registry**: AWS ECR
- **Monitoring**: OpenTelemetry + Grafana Stack

### Staging Environment
- **Kubernetes Cluster**: AWS EKS (separate namespace)
- **Database**: AWS RDS PostgreSQL (separate instance)
- **Load Balancer**: AWS Application Load Balancer
- **Container Registry**: AWS ECR

### Development Environment
- **Local**: Docker Compose
- **Database**: Local PostgreSQL
- **Services**: All services running locally

## Prerequisites

### Required Tools
- Docker Desktop
- kubectl
- AWS CLI
- Terraform
- Helm
- Git

### Required Accounts
- AWS Account with appropriate permissions
- Docker Hub or AWS ECR account
- GitHub account for CI/CD

## Local Development Setup

### Docker Compose Deployment

#### 1. Clone Repository
```bash
git clone https://github.com/your-org/enterprise-web-framework.git
cd enterprise-web-framework
```

#### 2. Environment Configuration
```bash
# Copy environment files
cp src/frontend/.env.example src/frontend/.env.local
cp src/backend/Core.API/appsettings.Development.json.example src/backend/Core.API/appsettings.Development.json

# Update configuration values
# - Database connection strings
# - Stripe API keys (test mode)
# - Google OAuth credentials
# - JWT secrets
```

#### 3. Start Services
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

#### 4. Database Setup
```bash
# Run database migrations
docker-compose exec backend dotnet ef database update

# Seed initial data
docker-compose exec backend dotnet run --project Core.Database --seed
```

### Service URLs
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **Database**: localhost:5432
- **Redis**: localhost:6379

## Staging Deployment

### 1. Infrastructure Setup

#### Terraform Configuration
```bash
# Navigate to terraform directory
cd terraform/staging

# Initialize Terraform
terraform init

# Plan infrastructure
terraform plan

# Apply infrastructure
terraform apply
```

#### Kubernetes Cluster Setup
```bash
# Configure kubectl
aws eks update-kubeconfig --region us-west-2 --name enterprise-web-framework-staging

# Verify cluster access
kubectl get nodes
```

### 2. Application Deployment

#### Build and Push Images
```bash
# Build backend image
docker build -f docker/Dockerfile.backend -t enterprise-web-framework/backend:staging .

# Build frontend image
docker build -f docker/Dockerfile.frontend -t enterprise-web-framework/frontend:staging .

# Push to ECR
aws ecr get-login-password --region us-west-2 | docker login --username AWS --password-stdin 123456789012.dkr.ecr.us-west-2.amazonaws.com

docker tag enterprise-web-framework/backend:staging 123456789012.dkr.ecr.us-west-2.amazonaws.com/enterprise-web-framework/backend:staging
docker tag enterprise-web-framework/frontend:staging 123456789012.dkr.ecr.us-west-2.amazonaws.com/enterprise-web-framework/frontend:staging

docker push 123456789012.dkr.ecr.us-west-2.amazonaws.com/enterprise-web-framework/backend:staging
docker push 123456789012.dkr.ecr.us-west-2.amazonaws.com/enterprise-web-framework/frontend:staging
```

#### Deploy to Kubernetes
```bash
# Apply Kubernetes manifests
kubectl apply -f kubernetes/staging/

# Verify deployment
kubectl get pods -n enterprise-web-framework-staging
kubectl get services -n enterprise-web-framework-staging
```

### 3. Database Setup

#### RDS Configuration
```bash
# Connect to RDS instance
psql -h enterprise-web-framework-staging.cluster-xyz.us-west-2.rds.amazonaws.com -U admin -d enterprise_web_framework

# Run migrations
dotnet ef database update --connection "Host=enterprise-web-framework-staging.cluster-xyz.us-west-2.rds.amazonaws.com;Database=enterprise_web_framework;Username=admin;Password=your-password"
```

## Production Deployment

### 1. Infrastructure Setup

#### Terraform Configuration
```bash
# Navigate to terraform directory
cd terraform/production

# Initialize Terraform
terraform init

# Plan infrastructure
terraform plan

# Apply infrastructure
terraform apply
```

#### Kubernetes Cluster Setup
```bash
# Configure kubectl
aws eks update-kubeconfig --region us-west-2 --name enterprise-web-framework-production

# Verify cluster access
kubectl get nodes
```

### 2. Application Deployment

#### Build and Push Images
```bash
# Build backend image
docker build -f docker/Dockerfile.backend -t enterprise-web-framework/backend:production .

# Build frontend image
docker build -f docker/Dockerfile.frontend -t enterprise-web-framework/frontend:production .

# Push to ECR
aws ecr get-login-password --region us-west-2 | docker login --username AWS --password-stdin 123456789012.dkr.ecr.us-west-2.amazonaws.com

docker tag enterprise-web-framework/backend:production 123456789012.dkr.ecr.us-west-2.amazonaws.com/enterprise-web-framework/backend:production
docker tag enterprise-web-framework/frontend:production 123456789012.dkr.ecr.us-west-2.amazonaws.com/enterprise-web-framework/frontend:production

docker push 123456789012.dkr.ecr.us-west-2.amazonaws.com/enterprise-web-framework/backend:production
docker push 123456789012.dkr.ecr.us-west-2.amazonaws.com/enterprise-web-framework/frontend:production
```

#### Deploy to Kubernetes
```bash
# Apply Kubernetes manifests
kubectl apply -f kubernetes/production/

# Verify deployment
kubectl get pods -n enterprise-web-framework-production
kubectl get services -n enterprise-web-framework-production
```

### 3. Database Setup

#### RDS Configuration
```bash
# Connect to RDS instance
psql -h enterprise-web-framework-production.cluster-xyz.us-west-2.rds.amazonaws.com -U admin -d enterprise_web_framework

# Run migrations
dotnet ef database update --connection "Host=enterprise-web-framework-production.cluster-xyz.us-west-2.rds.amazonaws.com;Database=enterprise_web_framework;Username=admin;Password=your-password"
```

## CI/CD Pipeline

### GitHub Actions Workflow

#### Backend Pipeline
```yaml
name: Backend CI/CD

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal

  build-and-deploy:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v3
      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-west-2
      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v1
      - name: Build, tag, and push image to Amazon ECR
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: enterprise-web-framework/backend
          IMAGE_TAG: ${{ github.sha }}
        run: |
          docker build -f docker/Dockerfile.backend -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG .
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG
      - name: Deploy to EKS
        run: |
          aws eks update-kubeconfig --region us-west-2 --name enterprise-web-framework-production
          kubectl set image deployment/backend backend=$ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG -n enterprise-web-framework-production
```

#### Frontend Pipeline
```yaml
name: Frontend CI/CD

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      - name: Install dependencies
        run: |
          cd src/frontend
          npm ci
      - name: Lint
        run: |
          cd src/frontend
          npm run lint
      - name: Type check
        run: |
          cd src/frontend
          npm run type-check
      - name: Test
        run: |
          cd src/frontend
          npm run test

  build-and-deploy:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v3
      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-west-2
      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v1
      - name: Build, tag, and push image to Amazon ECR
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: enterprise-web-framework/frontend
          IMAGE_TAG: ${{ github.sha }}
        run: |
          docker build -f docker/Dockerfile.frontend -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG .
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG
      - name: Deploy to EKS
        run: |
          aws eks update-kubeconfig --region us-west-2 --name enterprise-web-framework-production
          kubectl set image deployment/frontend frontend=$ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG -n enterprise-web-framework-production
```

## Kubernetes Manifests

### Namespace
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: enterprise-web-framework-production
  labels:
    name: enterprise-web-framework-production
```

### ConfigMap
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: app-config
  namespace: enterprise-web-framework-production
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:8080"
  DATABASE_PROVIDER: "PostgreSQL"
  REDIS_CONNECTION_STRING: "redis://redis-service:6379"
```

### Secret
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: app-secrets
  namespace: enterprise-web-framework-production
type: Opaque
data:
  DATABASE_CONNECTION_STRING: <base64-encoded-connection-string>
  JWT_SECRET: <base64-encoded-jwt-secret>
  STRIPE_SECRET_KEY: <base64-encoded-stripe-key>
  GOOGLE_CLIENT_SECRET: <base64-encoded-google-secret>
```

### Deployment
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: backend
  namespace: enterprise-web-framework-production
spec:
  replicas: 3
  selector:
    matchLabels:
      app: backend
  template:
    metadata:
      labels:
        app: backend
    spec:
      containers:
      - name: backend
        image: 123456789012.dkr.ecr.us-west-2.amazonaws.com/enterprise-web-framework/backend:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          valueFrom:
            configMapKeyRef:
              name: app-config
              key: ASPNETCORE_ENVIRONMENT
        - name: DATABASE_CONNECTION_STRING
          valueFrom:
            secretKeyRef:
              name: app-secrets
              key: DATABASE_CONNECTION_STRING
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

### Service
```yaml
apiVersion: v1
kind: Service
metadata:
  name: backend-service
  namespace: enterprise-web-framework-production
spec:
  selector:
    app: backend
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: ClusterIP
```

### Ingress
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: app-ingress
  namespace: enterprise-web-framework-production
  annotations:
    kubernetes.io/ingress.class: alb
    alb.ingress.kubernetes.io/scheme: internet-facing
    alb.ingress.kubernetes.io/target-type: ip
    alb.ingress.kubernetes.io/ssl-redirect: '443'
    alb.ingress.kubernetes.io/certificate-arn: arn:aws:acm:us-west-2:123456789012:certificate/12345678-1234-1234-1234-123456789012
spec:
  rules:
  - host: api.yourdomain.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: backend-service
            port:
              number: 80
```

## Monitoring and Observability

### OpenTelemetry Configuration
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: otel-config
  namespace: enterprise-web-framework-production
data:
  otel-collector.yaml: |
    receivers:
      otlp:
        protocols:
          grpc:
            endpoint: 0.0.0.0:4317
          http:
            endpoint: 0.0.0.0:4318
    
    processors:
      batch:
        timeout: 1s
        send_batch_size: 1024
    
    exporters:
      prometheus:
        endpoint: "0.0.0.0:8889"
      jaeger:
        endpoint: jaeger-collector:14250
        tls:
          insecure: true
    
    service:
      pipelines:
        traces:
          receivers: [otlp]
          processors: [batch]
          exporters: [jaeger]
        metrics:
          receivers: [otlp]
          processors: [batch]
          exporters: [prometheus]
```

### Grafana Stack Deployment
```bash
# Add Grafana Helm repository
helm repo add grafana https://grafana.github.io/helm-charts
helm repo update

# Install Grafana Stack
helm install grafana grafana/grafana-stack \
  --namespace monitoring \
  --create-namespace \
  --values kubernetes/monitoring/grafana-values.yaml
```

## Security Considerations

### Network Security
- Use VPC with private subnets
- Implement security groups
- Use Network Policies in Kubernetes
- Enable VPC Flow Logs

### Application Security
- Use HTTPS everywhere
- Implement proper authentication
- Use secrets management
- Regular security scans

### Data Security
- Encrypt data at rest
- Use SSL/TLS for data in transit
- Implement backup encryption
- Regular security audits

## Troubleshooting

### Common Issues

#### Pod Startup Issues
```bash
# Check pod status
kubectl get pods -n enterprise-web-framework-production

# View pod logs
kubectl logs -f deployment/backend -n enterprise-web-framework-production

# Describe pod for events
kubectl describe pod <pod-name> -n enterprise-web-framework-production
```

#### Database Connection Issues
```bash
# Test database connectivity
kubectl exec -it deployment/backend -n enterprise-web-framework-production -- psql $DATABASE_CONNECTION_STRING

# Check database logs
kubectl logs -f deployment/backend -n enterprise-web-framework-production | grep -i database
```

#### Performance Issues
```bash
# Check resource usage
kubectl top pods -n enterprise-web-framework-production

# Check node resources
kubectl top nodes

# View metrics
kubectl get --raw /metrics
```

### Rollback Procedures
```bash
# Rollback deployment
kubectl rollout undo deployment/backend -n enterprise-web-framework-production

# Check rollout status
kubectl rollout status deployment/backend -n enterprise-web-framework-production

# View rollout history
kubectl rollout history deployment/backend -n enterprise-web-framework-production
```

## Maintenance

### Regular Tasks
- Update dependencies
- Security patches
- Performance monitoring
- Backup verification
- Log rotation

### Scheduled Maintenance
- Weekly security scans
- Monthly dependency updates
- Quarterly performance reviews
- Annual security audits

## Support and Documentation

### Resources
- **Documentation**: https://docs.yourdomain.com
- **Status Page**: https://status.yourdomain.com
- **Support Email**: support@yourdomain.com
- **Slack Channel**: #enterprise-web-framework

### Emergency Contacts
- **On-Call Engineer**: +1-555-0123
- **DevOps Team**: devops@yourdomain.com
- **Security Team**: security@yourdomain.com
