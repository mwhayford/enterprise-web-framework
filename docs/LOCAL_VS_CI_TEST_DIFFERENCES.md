z# Local vs CI Test Execution Differences

This document explains why E2E tests may behave differently when run locally versus in GitHub Actions CI.

## Key Differences

### 1. **Playwright Configuration** (`playwright.config.ts`)

| Setting | Local | CI |
|---------|-------|-----|
| **Workers** | `undefined` (auto, typically uses all CPU cores) | `1` (sequential) |
| **Retries** | `0` (no retries) | `2` (retries failed tests twice) |
| **webServer** | Starts `docker-compose up` automatically | `undefined` (services started separately) |
| **forbidOnly** | `false` (allows `.only()` tests) | `true` (prevents `.only()` tests) |

**Impact:**
- **Local**: Tests run in parallel, faster execution, immediate failure on bugs
- **CI**: Tests run sequentially (more stable), failures get 2 retry attempts (handles flakiness)

### 2. **Docker Compose Files**

| Aspect | Local (`docker-compose.yml`) | CI (`docker-compose.ci.yml`) |
|--------|------------------------------|------------------------------|
| **Services** | Full stack (Postgres, Redis, Elasticsearch, Kafka, Prometheus, Jaeger) | Minimal (Postgres, Redis, Elasticsearch, Kafka) |
| **Elasticsearch** | Enabled | Disabled (`Elasticsearch__Disabled=true`) |
| **Kafka** | Enabled | Disabled (`Kafka__Disabled=true`) |
| **Hangfire** | Enabled | Disabled (`Hangfire__Disabled=true`) |
| **JWT Key** | `DevJwtKeyForDocker2024RentalManagerAppSecureMin32Chars!` | `YourSuperSecretKeyThatIsAtLeast32CharactersLong!` (placeholder allowed in CI) |
| **Container Names** | `rentalmanager-*` | `rentalmanager-*-ci` |
| **Volumes** | Persistent (`postgres_data`, `redis_data`) | Ephemeral (no volumes) |

**Impact:**
- **Local**: More services, persistent data, realistic environment
- **CI**: Minimal services, fresh database each run, faster startup

### 3. **Service Startup Timing**

| Aspect | Local | CI |
|--------|-------|-----|
| **Automatic Startup** | Playwright waits up to 120 seconds for `docker-compose up` | Manual startup with fixed 60-second wait |
| **Health Checks** | Playwright's `webServer` checks `http://localhost:3001` | Manual health check loops (30 attempts × 3 seconds = 90 seconds) |
| **Backend Start Period** | 40 seconds | 90 seconds |
| **Container Initialization** | Managed by Playwright | Explicit 60-second sleep after startup |

**Impact:**
- **Local**: Services may start faster/slower depending on your machine
- **CI**: Fixed timing, more predictable but may be slower

### 4. **Environment Variables**

| Variable | Local | CI |
|----------|-------|-----|
| `CI` | Not set | `true` |
| `BASE_URL` | Not set (defaults to `http://localhost:3001`) | Explicitly set to `http://localhost:3001` |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Development` |
| `DOTNET_RUNNING_IN_CONTAINER` | `true` | `true` |

**Impact:**
- CI-specific behaviors triggered by `CI=true` flag
- Explicit BASE_URL ensures correct URL even if defaults change

### 5. **Test Execution Flow**

#### Local Execution:
```bash
npm test
↓
Playwright detects webServer config
↓
Starts docker-compose (waits up to 120s)
↓
Tests run in parallel (auto workers)
↓
No retries (fails immediately)
↓
docker-compose stops automatically
```

#### CI Execution:
```bash
Docker services started manually (docker-compose.ci.yml)
↓
Wait 60 seconds for initialization
↓
Health check loops (30 attempts)
↓
Install Playwright dependencies
↓
Run tests sequentially (1 worker)
↓
Retry failed tests 2 times
↓
Stop services manually (docker-compose down)
```

### 6. **Resource Constraints**

| Aspect | Local | CI |
|--------|-------|-----|
| **CPU** | Your machine's CPU (likely more cores) | GitHub Actions: 2 cores |
| **Memory** | Your machine's RAM | GitHub Actions: 7 GB |
| **Network** | Local network (low latency) | GitHub Actions network (variable latency) |
| **Disk** | Local SSD (fast) | GitHub Actions ephemeral storage (slower) |

**Impact:**
- **Local**: Generally faster, but may have different resource contention
- **CI**: Limited resources, may cause timeouts or slower execution

### 7. **Database State**

| Aspect | Local | CI |
|--------|-------|-----|
| **Persistence** | Volumes persist data between runs | Fresh database each run |
| **Seeding** | May have existing seed data | Fresh database, may need seeding |
| **Test Isolation** | Previous test data may exist | Clean state each run |

**Impact:**
- **Local**: Tests may pass due to existing data or fail due to conflicts
- **CI**: Clean state, more reliable but may miss issues with existing data

### 8. **Browser Environment**

| Aspect | Local | CI |
|--------|-------|-----|
| **Display** | Your monitor (visible) | Headless (no display) |
| **GPU** | Hardware acceleration | Software rendering |
| **Fonts** | System fonts installed | Limited fonts |
| **Screen Size** | Your resolution | Standard (1920x1080) |

**Impact:**
- **Local**: Can see what's happening, may behave differently with GPU
- **CI**: Headless mode, may have different rendering behavior

## Common Issues & Solutions

### Issue 1: Tests Pass Locally but Fail in CI

**Causes:**
- Race conditions (parallel execution locally vs sequential in CI)
- Timing differences (local machine faster/slower)
- Different database state
- Resource constraints in CI

**Solutions:**
- Add explicit waits instead of fixed timeouts
- Use `waitFor` helpers instead of `setTimeout`
- Ensure tests are independent (no shared state)
- Mock external services consistently

### Issue 2: Tests Fail Locally but Pass in CI

**Causes:**
- Retries in CI mask flaky tests
- Different service configurations
- Local cache or stale data

**Solutions:**
- Run tests locally with `CI=true npm test` to match CI behavior
- Clear local Docker volumes: `docker-compose down -v`
- Check for environment-specific code paths

### Issue 3: Timeout Issues

**Causes:**
- CI has limited resources (slower execution)
- Services take longer to start in CI
- Network latency in CI

**Solutions:**
- Increase timeouts for CI-specific operations
- Add proper health checks before running tests
- Use `waitForURL` instead of fixed waits

### Issue 4: Authentication/Session Issues

**Causes:**
- Different JWT keys between environments
- Different authentication flow timing
- Session storage differences

**Solutions:**
- Ensure test authentication setup is identical
- Mock authentication consistently
- Use same token format in both environments

## Best Practices

1. **Run CI Mode Locally**:
   ```bash
   CI=true npm test
   ```

2. **Use Same Docker Compose for Testing**:
   ```bash
   docker-compose -f docker-compose.ci.yml up -d
   CI=true npm test
   ```

3. **Add Debug Logging**:
   - Use Playwright's trace viewer: `npx playwright show-trace`
   - Check screenshots on failure
   - Review container logs

4. **Write Deterministic Tests**:
   - No hardcoded waits (use `waitFor`)
   - Clear assertions
   - Independent test execution

5. **Mock External Services**:
   - Use API route mocking (`page.route()`)
   - Don't rely on real backend state
   - Mock authentication consistently

## Debugging Checklist

When tests behave differently:

- [ ] Check if `CI=true` is set locally
- [ ] Verify Docker containers are running
- [ ] Compare Playwright config settings
- [ ] Check environment variables
- [ ] Review service health checks
- [ ] Compare database state
- [ ] Check network/port availability
- [ ] Review resource usage (CPU/memory)
- [ ] Check Playwright traces and screenshots
- [ ] Compare container logs

## Quick Comparison Command

Run tests in CI mode locally to match GitHub Actions:

```bash
# Stop any existing services
docker-compose down -v

# Start CI services
docker-compose -f docker-compose.ci.yml up -d

# Wait for services (match CI timing)
sleep 60

# Run health checks
curl http://localhost:5111/health
curl http://localhost:3001

# Run tests in CI mode
cd tests/RentalManager.E2ETests
CI=true BASE_URL=http://localhost:3001 npm test
```

This will closely match the CI environment behavior.

