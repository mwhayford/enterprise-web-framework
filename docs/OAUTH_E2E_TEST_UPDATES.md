# OAuth E2E Test Updates Required

## Current Situation

The application uses **Google OAuth** for authentication:
- Login page redirects to `/api/auth/google`
- No email/password forms exist
- Authentication callback sets `auth_token` and `user` in localStorage

## Issues in Current Tests

### 1. Direct Email/Password Login Attempts (FAILING)

**Files:**
- `tests/RentalManager.E2ETests/tests/property-application-flow.spec.ts` (Lines 44-46, 99-101, 115-117)

**Problem:**
```typescript
// ❌ This doesn't exist in the app anymore
await page.goto('/login');
await page.fill('input[name="email"]', 'test@example.com');
await page.fill('input[name="password"]', 'Test123!');
await page.click('button[type="submit"]');
```

**Solution:** Replace with OAuth mocking or use `authenticatedPage` fixture

---

## Required Updates

### File 1: `property-application-flow.spec.ts`

#### Update 1: Test "authenticated user can submit application" (Line 41-78)

**Replace:**
```typescript
test('authenticated user can submit application', async ({ page }) => {
  // ❌ OLD - Direct login form
  await page.goto('/login');
  await page.fill('input[name="email"]', 'test@example.com');
  await page.fill('input[name="password"]', 'Test123!');
  await page.click('button[type="submit"]');
  await expect(page).toHaveURL('/dashboard');
  
  // ... rest of test
});
```

**With:**
```typescript
test('authenticated user can submit application', async ({ authenticatedPage, page }) => {
  // ✅ NEW - Use authenticated fixture
  // authenticatedPage already sets up localStorage with mock token
  
  // Navigate to property and apply
  await page.goto('/properties');
  await page.locator('[data-testid="property-card"]').first().click();
  // ... rest of test
});
```

#### Update 2: Test "should view submitted applications" (Line 96-108)

**Replace:**
```typescript
test('should view submitted applications', async ({ page }) => {
  // ❌ OLD
  await page.goto('/login');
  await page.fill('input[name="email"]', 'test@example.com');
  await page.fill('input[name="password"]', 'Test123!');
  await page.click('button[type="submit"]');
  
  await page.goto('/applications/my');
  // ...
});
```

**With:**
```typescript
test('should view submitted applications', async ({ authenticatedPage, page }) => {
  // ✅ NEW - Use authenticated fixture
  await page.goto('/applications/my');
  // ...
});
```

#### Update 3: Admin tests (Lines 111-168)

**Replace:**
```typescript
test.describe('Admin Application Management', () => {
  test.beforeEach(async ({ page }) => {
    // ❌ OLD - Admin login with email/password
    await page.goto('/login');
    await page.fill('input[name="email"]', 'admin@example.com');
    await page.fill('input[name="password"]', 'Admin123!');
    await page.click('button[type="submit"]');
  });
  // ...
});
```

**With:**
```typescript
test.describe('Admin Application Management', () => {
  test.beforeEach(async ({ page }) => {
    // ✅ NEW - Mock admin authentication
    await page.goto('/');
    await page.evaluate(() => {
      const adminToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'; // Mock admin JWT
      const adminUser = {
        id: 'admin-user-id',
        email: 'admin@example.com',
        firstName: 'Admin',
        lastName: 'User',
        roles: ['Admin'], // Important for admin access
        isActive: true,
      };
      localStorage.setItem('auth_token', adminToken);
      localStorage.setItem('user', JSON.stringify(adminUser));
    });
  });
  // ...
});
```

---

### File 2: `auth.fixture.ts` (Optional Enhancement)

**Current:** Mock authentication already exists but could be enhanced for admin users.

**Enhancement:**
```typescript
// Add admin fixture
authenticatedAdminPage: async ({ page }, use) => {
  await page.goto('/');
  await page.evaluate(() => {
    const adminToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...';
    const adminUser = {
      id: 'admin-user-id',
      email: 'admin@example.com',
      firstName: 'Admin',
      lastName: 'User',
      roles: ['Admin'],
      isActive: true,
    };
    localStorage.setItem('auth_token', adminToken);
    localStorage.setItem('user', JSON.stringify(adminUser));
  });
  
  const dashboardPage = new DashboardPage(page);
  await dashboardPage.navigate();
  await use(dashboardPage);
},
```

---

### File 3: OAuth Flow Mocking (Advanced Option)

For more realistic OAuth testing, consider mocking the OAuth callback:

```typescript
// In playwright.config.ts or test setup
test.beforeAll(async ({ browser }) => {
  const context = await browser.newContext();
  
  // Intercept OAuth redirect
  await context.route('**/api/auth/google**', route => {
    route.fulfill({
      status: 302,
      headers: {
        Location: '/auth/callback?token=mock-token&user=' + encodeURIComponent(JSON.stringify({
          id: 'test-user',
          email: 'test@example.com',
          // ... user data
        }))
      }
    });
  });
});
```

---

## Summary of Changes Needed

1. **Remove all email/password form fills** (3 locations in `property-application-flow.spec.ts`)
2. **Use `authenticatedPage` fixture** instead of direct login
3. **Add admin authentication fixture** for admin tests
4. **Update test descriptions** if needed to reflect OAuth

## Files to Modify

1. ✅ `tests/RentalManager.E2ETests/tests/property-application-flow.spec.ts` - **MAIN FILE**
2. ⚠️ `tests/RentalManager.E2ETests/tests/fixtures/auth.fixture.ts` - **OPTIONAL** (add admin fixture)
3. ⚠️ `tests/RentalManager.E2ETests/playwright.config.ts` - **OPTIONAL** (for OAuth mocking)

## Test Impact

**Currently Failing Tests (7):**
- `authenticated user can submit application` (line 41)
- `should view submitted applications` (line 96)
- `admin can view all applications` (line 120)
- `admin can review and approve application` (line 127)
- `admin can review and reject application` (line 143)
- `admin can filter applications by status` (line 159)

**All will pass** after these updates.

