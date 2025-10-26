# Pre-Deployment Checklist

Use this checklist before deploying to AWS to ensure everything is properly configured and ready for production.

## Table of Contents

- [Prerequisites](#prerequisites)
- [AWS Account Setup](#aws-account-setup)
- [GitHub Configuration](#github-configuration)
- [Application Configuration](#application-configuration)
- [Security Checklist](#security-checklist)
- [Testing Checklist](#testing-checklist)
- [Documentation Review](#documentation-review)
- [Final Verification](#final-verification)

## Prerequisites

### Development Tools

- [ ] AWS CLI installed and configured (v2.x+)
- [ ] Terraform installed (v1.6.0+)
- [ ] kubectl installed (v1.28+)
- [ ] Docker installed and running
- [ ] Git configured with proper credentials
- [ ] Code editor (VS Code, Visual Studio, etc.)

### AWS Account

- [ ] AWS account created and billing enabled
- [ ] AWS account ID documented
- [ ] Root account secured with MFA
- [ ] AWS CLI configured with appropriate credentials
  ```bash
  aws sts get-caller-identity  # Should return your AWS account info
  ```

### Repository Access

- [ ] GitHub repository forked/cloned
- [ ] Git remote configured correctly
- [ ] Access to repository settings (for secrets)
- [ ] GitHub Actions enabled

## AWS Account Setup

### IAM Configuration

- [ ] IAM user created for GitHub Actions deployment
  - User name: `github-actions-deployer`
  - Access type: Programmatic access
- [ ] Required IAM policies attached
  - [ ] PowerUserAccess (or custom least-privilege policy)
  - [ ] IAMFullAccess (for Terraform IAM operations)
- [ ] Access key created and saved securely
- [ ] Access key tested with AWS CLI

### S3 Backend Setup

- [ ] S3 bucket created for Terraform state
  - Bucket name: `core-terraform-state`
  - Region: `us-east-1` (or your preferred region)
- [ ] S3 versioning enabled
- [ ] S3 encryption enabled (AES-256)
- [ ] S3 public access blocked
- [ ] DynamoDB table created for state locking
  - Table name: `core-terraform-locks`
  - Primary key: `LockID` (String)

**Command to verify**:
```bash
# Run setup script
./infrastructure/terraform/setup-backend.sh  # Linux/macOS
.\infrastructure\terraform\setup-backend.ps1  # Windows
```

### ECR Repositories (Optional)

- [ ] ECR repository created for backend: `core-backend`
- [ ] ECR repository created for frontend: `rentalmanager-frontend`
- [ ] Lifecycle policies configured
- [ ] Image scanning enabled

## GitHub Configuration

### Repository Secrets

- [ ] `AWS_ACCESS_KEY_ID` configured
- [ ] `AWS_SECRET_ACCESS_KEY` configured
- [ ] Secrets tested (trigger a workflow manually)

### Environment Secrets

#### Staging Environment
- [ ] Environment `staging` created in GitHub
- [ ] `EKS_CLUSTER_NAME` = `core-staging-eks`
- [ ] `APP_URL` = your staging frontend URL
- [ ] `API_URL` = your staging API URL

#### Production Environment
- [ ] Environment `production` created in GitHub
- [ ] `EKS_CLUSTER_NAME` = `core-production-eks`
- [ ] `APP_URL` = your production frontend URL
- [ ] `API_URL` = your production API URL
- [ ] Environment protection rules configured
  - [ ] Required reviewers added
  - [ ] Wait timer configured (optional)

### Workflow Configuration

- [ ] GitHub Actions workflows reviewed
  - [ ] `.github/workflows/ci.yml`
  - [ ] `.github/workflows/cd.yml`
- [ ] Branch protection rules configured
  - [ ] Require PR reviews before merging to main
  - [ ] Require status checks to pass
- [ ] Workflow permissions verified

## Application Configuration

### Backend Configuration

- [ ] `appsettings.json` reviewed
- [ ] `appsettings.Production.json` created with production values
- [ ] Connection strings configured (will be set via Kubernetes secrets)
- [ ] JWT secret key generated
  ```bash
  openssl rand -base64 64
  ```
- [ ] Stripe API keys obtained (test and production)
- [ ] Google OAuth credentials configured
  - [ ] Client ID
  - [ ] Client Secret
  - [ ] Authorized redirect URIs updated

### Frontend Configuration

- [ ] `.env.production` created
- [ ] API URL configured
- [ ] Stripe publishable key configured
- [ ] Google OAuth client ID configured
- [ ] Build verified locally
  ```bash
  cd src/frontend
  npm run build
  ```

### Database

- [ ] Database schema reviewed
- [ ] Migrations tested locally
- [ ] Seed data prepared (if needed)
- [ ] Backup/restore procedures documented

### Infrastructure as Code

- [ ] Terraform variables reviewed
  - [ ] `infrastructure/terraform/environments/staging/variables.tf`
  - [ ] `infrastructure/terraform/environments/production/variables.tf`
- [ ] Resource sizing appropriate for workload
- [ ] Cost estimates reviewed
- [ ] Tags configured for cost allocation

## Security Checklist

### Secrets Management

- [ ] All secrets stored securely (never in code)
- [ ] Production secrets different from staging/development
- [ ] Database passwords strong and unique
- [ ] API keys rotated regularly
- [ ] Secrets documented in secure location (1Password, LastPass, etc.)

### Authentication & Authorization

- [ ] Google OAuth application configured
  - [ ] Production OAuth app created (separate from dev)
  - [ ] Authorized redirect URIs updated
  - [ ] Privacy policy and terms of service links added
- [ ] JWT token expiration configured (60 minutes recommended)
- [ ] Refresh token mechanism working
- [ ] Role-based access control implemented

### Network Security

- [ ] Security groups configured with least privilege
- [ ] RDS not publicly accessible
- [ ] Redis not publicly accessible
- [ ] ALB configured with HTTPS
- [ ] SSL/TLS certificates obtained
  - [ ] Certificate requested in ACM
  - [ ] DNS validation completed
- [ ] CORS policies configured correctly

### Application Security

- [ ] Input validation implemented
- [ ] SQL injection protection (parameterized queries)
- [ ] XSS protection enabled
- [ ] CSRF protection enabled
- [ ] Rate limiting configured
- [ ] Security headers configured
  - [ ] HSTS
  - [ ] X-Content-Type-Options
  - [ ] X-Frame-Options
  - [ ] CSP

### Compliance

- [ ] Data encryption at rest (RDS, Redis, S3)
- [ ] Data encryption in transit (TLS)
- [ ] Logging configured for audit trail
- [ ] GDPR compliance reviewed (if applicable)
- [ ] PCI DSS compliance reviewed (for Stripe payments)

## Testing Checklist

### Unit Tests

- [ ] All unit tests passing
  ```bash
  cd tests/RentalManager.UnitTests
  dotnet test
  ```
- [ ] Code coverage > 80%
- [ ] Critical paths tested

### Integration Tests

- [ ] All integration tests passing
  ```bash
  cd tests/RentalManager.IntegrationTests
  dotnet test
  ```
- [ ] Database integration tested
- [ ] Redis integration tested
- [ ] External service integrations tested

### End-to-End Tests

- [ ] E2E tests passing
  ```bash
  cd tests/RentalManager.E2ETests
  npm test
  ```
- [ ] Authentication flow tested
- [ ] Payment flow tested
- [ ] Subscription flow tested
- [ ] Critical user journeys tested

### Manual Testing

- [ ] User registration working
- [ ] Login working (email and Google OAuth)
- [ ] Payment processing working (test mode)
- [ ] Subscription management working
- [ ] Search functionality working
- [ ] Error handling tested
- [ ] Mobile responsiveness verified

### Performance Testing

- [ ] Load testing performed
- [ ] Database query optimization
- [ ] API response times acceptable (<500ms)
- [ ] Frontend load time acceptable (<3s)
- [ ] Auto-scaling tested

## Documentation Review

- [ ] README.md up to date
- [ ] Architecture documentation reviewed
- [ ] API documentation complete (Swagger)
- [ ] Database schema documented
- [ ] Deployment guide reviewed
- [ ] Runbooks created for common operations
  - [ ] Deployment procedure
  - [ ] Rollback procedure
  - [ ] Incident response procedure
  - [ ] Database backup/restore procedure

## Final Verification

### Local Environment

- [ ] Application runs successfully with Docker Compose
- [ ] All services healthy
- [ ] Frontend accessible at http://localhost:3001
- [ ] Backend API accessible at http://localhost:5111
- [ ] Swagger UI accessible at http://localhost:5111/swagger

### Staging Deployment

- [ ] Terraform plan reviewed (no unexpected changes)
- [ ] Infrastructure deployed successfully
- [ ] Application deployed to Kubernetes
- [ ] All pods running and healthy
- [ ] Health checks passing
- [ ] Database migrations applied
- [ ] Smoke tests passing
- [ ] Application accessible via ALB
- [ ] SSL certificate working
- [ ] DNS configured correctly
- [ ] Logs visible in CloudWatch
- [ ] Metrics visible in CloudWatch/Prometheus

### Production Readiness

- [ ] Staging environment tested thoroughly
- [ ] Production configuration reviewed
- [ ] Production secrets updated
- [ ] Production domain configured
- [ ] SSL certificate for production domain
- [ ] Backup procedures tested
- [ ] Disaster recovery plan documented
- [ ] Monitoring and alerting configured
- [ ] On-call rotation established
- [ ] Incident response plan ready

### Team Readiness

- [ ] Team trained on deployment procedures
- [ ] Access controls documented
- [ ] Emergency contacts list created
- [ ] Communication plan for deployment
- [ ] Rollback plan communicated
- [ ] Post-deployment verification plan

## Deployment Approval

### Staging Approval

- [ ] Technical lead approval
- [ ] Staging deployment scheduled
- [ ] Stakeholders notified

### Production Approval

- [ ] Product owner approval
- [ ] Technical lead approval
- [ ] Security review completed
- [ ] Change advisory board approval (if applicable)
- [ ] Production deployment scheduled
- [ ] Maintenance window communicated to users
- [ ] Stakeholders and customers notified

## Post-Deployment Checklist

### Immediate Verification (Within 1 hour)

- [ ] Application accessible
- [ ] Health checks passing
- [ ] Critical user flows working
- [ ] No error spikes in logs
- [ ] No alarm triggers in monitoring
- [ ] Database performance acceptable
- [ ] Cache hit rate acceptable

### Extended Verification (Within 24 hours)

- [ ] All features working as expected
- [ ] Performance metrics within acceptable range
- [ ] No security incidents
- [ ] Customer feedback positive
- [ ] Support tickets normal volume
- [ ] Cost monitoring aligned with estimates

### Follow-Up (Within 1 week)

- [ ] Full regression testing completed
- [ ] Performance optimization identified
- [ ] Documentation updated with lessons learned
- [ ] Retrospective meeting scheduled
- [ ] Next deployment improvements identified

## Emergency Contacts

Document your emergency contacts:

| Role | Name | Phone | Email |
|------|------|-------|-------|
| Technical Lead | | | |
| DevOps Engineer | | | |
| Database Admin | | | |
| Security Officer | | | |
| On-Call Engineer | | | |

## Rollback Criteria

Rollback should be initiated if:
- [ ] Health checks failing for > 5 minutes
- [ ] Error rate > 5% for > 2 minutes
- [ ] Critical functionality broken
- [ ] Security vulnerability discovered
- [ ] Database corruption detected
- [ ] Performance degradation > 50%

## Sign-Off

### Staging Deployment

- **Deployed by**: _________________ Date: _______
- **Verified by**: _________________ Date: _______
- **Approved by**: _________________ Date: _______

### Production Deployment

- **Deployed by**: _________________ Date: _______
- **Verified by**: _________________ Date: _______
- **Approved by**: _________________ Date: _______

---

**Note**: This checklist should be customized based on your organization's specific requirements, compliance needs, and deployment policies.

## Additional Resources

- [AWS Deployment Guide](aws-deployment.md)
- [GitHub Secrets Configuration](github-secrets.md)
- [CI/CD Documentation](ci-cd.md)
- [Monitoring Guide](observability-testing.md)
- [Security Best Practices](https://aws.amazon.com/architecture/security-identity-compliance/)


