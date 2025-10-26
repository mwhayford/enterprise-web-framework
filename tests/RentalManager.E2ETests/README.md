# Core E2E Tests

End-to-end tests for the Core application using Playwright.

## Setup

```bash
cd tests/RentalManager.E2ETests
npm install
npx playwright install chromium
```

## Running Tests

### Run all tests (headless)
```bash
npm test
```

### Run tests with browser visible
```bash
npm run test:headed
```

### Run tests in debug mode
```bash
npm run test:debug
```

### Run tests with UI mode
```bash
npm run test:ui
```

### Run specific test suites
```bash
npm run test:auth          # Authentication tests
npm run test:search        # Search functionality tests
npm run test:payment       # Payment processing tests
npm run test:subscription  # Subscription management tests
npm run test:navigation    # Navigation and routing tests
```

### View test report
```bash
npm run report
```

### Generate tests using Codegen
```bash
npm run codegen
```

## Test Structure

```
tests/
├── fixtures/          # Test fixtures and shared setup
│   └── auth.fixture.ts
├── pages/             # Page Object Models
│   ├── BasePage.ts
│   ├── LoginPage.ts
│   ├── DashboardPage.ts
│   └── SearchPage.ts
├── auth.spec.ts       # Authentication flow tests
├── search.spec.ts     # Search functionality tests
├── payment.spec.ts    # Payment processing tests
├── subscription.spec.ts # Subscription management tests
└── navigation.spec.ts # Navigation and routing tests
```

## Test Coverage

### Critical User Journeys

1. **Authentication Flow**
   - Login with Google OAuth
   - Session persistence
   - Protected route access
   - Logout functionality

2. **Search Functionality**
   - Search across different indices
   - Filter and pagination
   - Empty state handling
   - Result display

3. **Payment Processing**
   - One-time payments
   - Payment validation
   - Stripe integration
   - Payment methods management

4. **Subscription Management**
   - Create subscriptions
   - View active subscriptions
   - Cancel subscriptions
   - Subscription plans

5. **Navigation**
   - Between pages
   - Browser history
   - Deep linking
   - 404 handling

## Configuration

Tests are configured in `playwright.config.ts`:

- **Base URL**: `http://localhost:3001` (can be overridden with `BASE_URL` env var)
- **Browser**: Chromium (Desktop Chrome)
- **Retries**: 2 retries in CI, 0 locally
- **Reporters**: HTML, JSON, JUnit
- **Screenshots**: On failure
- **Videos**: On failure
- **Traces**: On first retry

## CI/CD Integration

Tests are designed to run in CI environments:

```bash
CI=true npm test
```

In CI mode:
- Tests run in parallel (1 worker)
- Retries are enabled (2 retries)
- `--forbid-only` prevents `.only()` tests from running

## Prerequisites

Before running E2E tests, ensure:

1. Docker containers are running (`docker-compose up`)
2. Frontend is accessible at `http://localhost:3001`
3. Backend API is accessible at `http://localhost:5111`
4. Database is seeded with test data (if required)

## Debugging

### Visual debugging
```bash
npm run test:debug
```

### UI Mode (interactive)
```bash
npm run test:ui
```

### View traces
After a test failure, traces are saved in `test-results/`. Open them with:
```bash
npx playwright show-trace test-results/<trace-file>.zip
```

## Best Practices

1. **Page Object Model**: All page interactions are abstracted into page objects
2. **Fixtures**: Common setup (like authentication) is handled in fixtures
3. **Selectors**: Use accessible selectors (text, aria-labels) over CSS selectors
4. **Waits**: Use built-in waits (`waitForLoadState`, `waitForURL`) instead of arbitrary timeouts
5. **Assertions**: Use Playwright's auto-retrying assertions
6. **Independence**: Tests should be independent and can run in any order
7. **Cleanup**: Tests clean up their own data when possible

## Mocking

For external services (like Stripe OAuth), tests use:
- Local storage mocking for authentication
- Test API keys for Stripe
- Mock responses for third-party APIs

## Troubleshooting

### Tests timing out
- Increase timeout in `playwright.config.ts`
- Check if Docker containers are running
- Verify frontend is accessible

### Flaky tests
- Add explicit waits for dynamic content
- Use `waitForLoadState('networkidle')`
- Check for race conditions

### Stripe Elements not loading
- Verify `VITE_STRIPE_PUBLISHABLE_KEY` is set
- Use Stripe test keys
- Check browser console for errors

## Resources

- [Playwright Documentation](https://playwright.dev)
- [Best Practices](https://playwright.dev/docs/best-practices)
- [Debugging Guide](https://playwright.dev/docs/debug)

