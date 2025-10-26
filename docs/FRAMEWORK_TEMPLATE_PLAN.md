# Framework Template Setup

## Phase 1: Implement Feature Flags System

### 1.1 Add Feature Configuration
- Create `src/backend/RentalManager.Domain/Configuration/FeatureFlags.cs` class with boolean properties for each optional feature:
  - `EnableKafka`, `EnableElasticsearch`, `EnableStripe`, `EnableHangfire`, `EnableObservability`
- Update `appsettings.json` and `appsettings.Development.json` with new `Features` section
- Add validation to ensure dependent features are enabled together (e.g., Stripe requires Hangfire for async processing)

### 1.2 Refactor Service Registration
- Update `src/backend/RentalManager.API/Program.cs` to conditionally register services based on feature flags:
  - Wrap Kafka/EventBus registration in `if (builder.Configuration.GetValue<bool>("Features:EnableKafka"))`
  - Wrap Elasticsearch registration in feature flag check
  - Wrap Stripe services in feature flag check
  - Wrap Hangfire in feature flag check
  - Wrap OpenTelemetry/Jaeger/Prometheus in feature flag check
- Ensure app runs without errors when features are disabled

### 1.3 Add Runtime Feature Checks
- Create `src/backend/RentalManager.Application/Services/FeatureService.cs` to check feature availability at runtime
- Update controllers to check features before executing optional functionality:
  - `PaymentsController` / `SubscriptionsController` check `EnableStripe`
  - `SearchController` checks `EnableElasticsearch`
  - Event handlers check `EnableKafka` before publishing
- Return appropriate HTTP responses (e.g., 501 Not Implemented) when features are disabled

### 1.4 Update Docker Compose
- Add comments in `docker-compose.yml` marking which services are optional
- Create `docker-compose.minimal.yml` with only core services (Postgres, Redis)
- Update docker-compose to gracefully handle missing optional services

## Phase 2: Create Comprehensive Documentation

### 2.1 Main README.md
- Add framework description and value proposition
- Add "Use This Template" button instructions
- Add feature matrix table showing what's included
- Add quick start guide (3-5 steps)
- Add architecture diagram (using mermaid or ASCII)
- Add link to detailed documentation

### 2.2 GETTING_STARTED.md
- Step-by-step setup instructions:
  1. Click "Use this template"
  2. Configure feature flags
  3. Set up local development environment
  4. Run migrations
  5. Start application
  6. Test with included Postman collection
- Include troubleshooting section

### 2.3 FEATURES.md
- Document each feature with:
  - Description and use case
  - Configuration options
  - Required services/dependencies
  - Cost implications (for AWS deployment)
  - How to enable/disable
  - Code examples

### 2.4 DEPLOYMENT.md
- Consolidate existing deployment docs
- Add cost calculator for different feature combinations
- Document infrastructure teardown procedures
- Add production readiness checklist

### 2.5 CONTRIBUTING.md
- Contribution guidelines
- Code style requirements
- PR process
- How to report issues
- Development workflow

### 2.6 CHANGELOG.md
- Create initial v1.0.0 entry
- Document versioning strategy (semantic versioning)
- Template for future updates

## Phase 3: Repository Configuration

### 3.1 GitHub Templates
- Create `.github/ISSUE_TEMPLATE/bug_report.md`
- Create `.github/ISSUE_TEMPLATE/feature_request.md`
- Create `.github/ISSUE_TEMPLATE/question.md`
- Create `.github/PULL_REQUEST_TEMPLATE.md`

### 3.2 Environment Template Files
- Create `.env.example` with all required environment variables (placeholder values)
- Create `appsettings.Template.json` showing all configuration options
- Update `.gitignore` to ensure secrets aren't committed

### 3.3 Branch Protection (Manual Steps Document)
Create `docs/REPOSITORY_SETUP.md` with instructions for:
- Enabling GitHub Template Repository (Settings → General → Template repository checkbox)
- Branch protection rules for `main`:
  - Require pull request before merging (1 approval)
  - Require status checks: CI pipeline must pass
  - Require branches to be up to date
  - Require linear history
  - Do not allow bypassing
- Add repository description: "Production-ready .NET 9 + React enterprise web application framework with authentication, payments, search, and AWS deployment"
- Add topics: `dotnet`, `react`, `typescript`, `aws`, `terraform`, `kubernetes`, `template`, `framework`, `enterprise`

### 3.4 Repository Metadata
- Update `LICENSE` (if needed)
- Add `CODE_OF_CONDUCT.md`
- Update repository settings for Issues, Discussions, Wiki as needed

## Phase 4: Clean Up Template-Specific Code

### 4.1 Remove Hardcoded Values
- Update Terraform configs to use variables instead of hardcoded AWS account ID (`436399375303`)
- Remove specific domain references (replace with `example.com` placeholders)
- Update `infrastructure/terraform/environments/staging/main.tf` S3 backend bucket to use variable
- Add `terraform.tfvars.example` files

### 4.2 Secrets Documentation
- Update `docs/github-secrets.md` with clear "REPLACE THIS" markers
- Create `docs/SECRETS_CHECKLIST.md` for new users
- Update Kubernetes manifests to use placeholders for secrets

### 4.3 Add Sample Data (Optional)
- Create seed data scripts for demo purposes
- Add sample Postman/Thunder Client collection for API testing
- Include example integration test scenarios

## Phase 5: Create Initial Release

### 5.1 Version Tagging
- Create git tag `v1.0.0`
- Push tag to GitHub

### 5.2 GitHub Release
Create `docs/RELEASE_NOTES_v1.0.0.md` with content for GitHub Release:
- Release highlights
- Feature list
- Breaking changes (none for v1.0.0)
- Known issues
- Upgrade instructions (N/A for initial)

### 5.3 Post-Release
- Test template creation flow
- Verify all CI/CD pipelines work from forked repo
- Update any broken links or references

## Key Files to Create/Modify

**New Files:**
- `src/backend/RentalManager.Domain/Configuration/FeatureFlags.cs`
- `src/backend/RentalManager.Application/Services/FeatureService.cs`
- `docker-compose.minimal.yml`
- `GETTING_STARTED.md`
- `FEATURES.md`
- `DEPLOYMENT.md`
- `CONTRIBUTING.md`
- `CHANGELOG.md`
- `CODE_OF_CONDUCT.md`
- `.env.example`
- `appsettings.Template.json`
- `.github/ISSUE_TEMPLATE/*.md`
- `.github/PULL_REQUEST_TEMPLATE.md`
- `docs/REPOSITORY_SETUP.md`
- `docs/SECRETS_CHECKLIST.md`
- `docs/RELEASE_NOTES_v1.0.0.md`
- `infrastructure/terraform/terraform.tfvars.example`

**Modified Files:**
- `README.md` - Complete rewrite for framework positioning
- `src/backend/RentalManager.API/Program.cs` - Conditional service registration
- `src/backend/RentalManager.API/appsettings.json` - Add Features section
- `src/backend/RentalManager.API/appsettings.Development.json` - Add Features section
- `src/backend/RentalManager.API/Controllers/*` - Add feature checks
- `infrastructure/terraform/environments/*/main.tf` - Use variables for account-specific values
- `docker-compose.yml` - Add comments for optional services

## Success Criteria

1. Any developer can click "Use this template" and have a working app in <30 minutes
2. All features can be toggled on/off without code changes
3. Application runs successfully with all features disabled (minimal mode)
4. Documentation clearly explains each feature and its purpose
5. GitHub branch protection prevents direct commits to main
6. Infrastructure costs are transparent and predictable


