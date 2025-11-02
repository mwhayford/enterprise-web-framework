# Playwright E2E Test Fix Plan

## Test Run Results Summary
- **Total Tests**: 41
- **Passed**: 19
- **Failed**: 22

## Root Cause Analysis

### 1. Navigation Element Mismatch (8 failures)
**Issue**: Tests are looking for buttons that don't exist. The UI uses navigation links, not standalone buttons.

**Affected Tests**:
- `navigation.spec.ts`: "should navigate between dashboard sections", "should maintain scroll position"
- `auth.spec.ts`: "should maintain authentication across page navigation"
- `payment.spec.ts`: "should display payment methods list", "should show add payment method button", "should display empty state"
- `subscription.spec.ts`: All 7 subscription tests

**Root Cause**: `DashboardPage.ts` has incorrect selectors:
- `button:has-text("Search Content")` - Should be `a[href="/search"]` or `link with text "Search"`
- `button:has-text("Payment Methods")` - Should be `a[href="/payment-methods"]`
- `button:has-text("Subscribe")` - Should be `a[href="/subscription"]`
- `button:has-text("Make Payment")` - Should be `a[href="/payment"]`

**Solution**: Update `DashboardPage.ts` to use navigation link selectors from `MainNavigation` component.

---

### 2. Authentication Method Mismatch (8 failures)
**Issue**: Tests attempt email/password login, but app only uses Google OAuth.

**Affected Tests**:
- `property-application-flow.spec.ts`: 
  - "authenticated user can submit application" (line 44)
  - "should view submitted applications" (line 99)
  - Admin tests (lines 115-117)

**Root Cause**: Tests use:
```typescript
await page.fill('input[name="email"]', 'test@example.com');
await page.fill('input[name="password"]', 'Test123!');
```
But `LoginPage.tsx` only has Google OAuth button.

**Solution**: Replace with `authenticatedPage` fixture which sets localStorage auth tokens.

---

### 3. Property Detail Page Selector Issue (2 failures)
**Issue**: Test can't find "Apply Now" button.

**Affected Tests**:
- `property-application-flow.spec.ts`: 
  - "should show property details" (line 26)
  - "should require authentication for application submission" (line 35)

**Root Cause**: Selector `text=Apply Now` may have whitespace/rendering issues, or button only appears when property status is available (status === 0).

**Solution**: Use more specific selector: `button:has-text("Apply Now")` or data-testid.

---

### 4. Filter Button Visibility Issue (1 failure)
**Issue**: Filter button not visible during test.

**Affected Test**:
- `property-application-flow.spec.ts`: "should filter properties by criteria" (line 84)

**Root Cause**: Filter button has `lg:hidden` class - only visible on mobile. Test likely runs on desktop viewport.

**Solution**: Either:
- Set mobile viewport: `await page.setViewportSize({ width: 375, height: 667 })`
- Or use desktop sidebar: `select[aria-label="Bedrooms"]` (visible on desktop)

---

## Implementation Plan

### Phase 1: Fix Navigation (Priority: HIGH)
**Files to Update**:
1. `tests/RentalManager.E2ETests/tests/pages/DashboardPage.ts`
   - Update `searchLink`, `paymentMethodsLink`, `subscriptionLink`, `paymentLink` selectors
   - Change from `button:has-text(...)` to navigation link selectors
   - Use `a[href="/path"]` or check MainNavigation structure

**Changes**:
```typescript
// OLD
private readonly searchLink = 'button:has-text("Search Content")';
private readonly paymentMethodsLink = 'button:has-text("Payment Methods")';

// NEW - Check MainNavigation.tsx for actual structure
private readonly searchLink = 'nav a[href="/search"], nav:has-text("Search")';
private readonly paymentMethodsLink = 'nav a[href*="payment-methods"]';
```

---

### Phase 2: Fix Authentication (Priority: HIGH)
**Files to Update**:
1. `tests/RentalManager.E2ETests/tests/property-application-flow.spec.ts`
   - Replace direct login attempts with `authenticatedPage` fixture
   - Update test signatures to include `authenticatedPage` fixture

**Changes**:
```typescript
// OLD
test('authenticated user can submit application', async ({ page }) => {
  await page.goto('/login');
  await page.fill('input[name="email"]', 'test@example.com');
  // ...
});

// NEW
test('authenticated user can submit application', async ({ authenticatedPage, page }) => {
  // authenticatedPage already logged in via fixture
  await page.goto('/properties');
  // ...
});
```

---

### Phase 3: Fix Property Detail Selectors (Priority: MEDIUM)
**Files to Update**:
1. `tests/RentalManager.E2ETests/tests/property-application-flow.spec.ts`
   - Update "Apply Now" button selector
   - Add check for property availability status

**Changes**:
```typescript
// OLD
await expect(page.locator('text=Apply Now')).toBeVisible();

// NEW
await expect(page.locator('button:has-text("Apply Now")')).toBeVisible();
// Or add data-testid to PropertyDetailPage.tsx button
```

---

### Phase 4: Fix Filter Visibility (Priority: MEDIUM)
**Files to Update**:
1. `tests/RentalManager.E2ETests/tests/property-application-flow.spec.ts`
   - Set mobile viewport OR use desktop sidebar selector

**Changes**:
```typescript
// Option 1: Mobile viewport
test('should filter properties by criteria', async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 667 });
  await page.goto('/properties');
  await page.click('[aria-label="Open filters"]');
  // ...
});

// Option 2: Desktop sidebar (easier)
test('should filter properties by criteria', async ({ page }) => {
  await page.goto('/properties');
  // Skip mobile button, use desktop sidebar directly
  await page.selectOption('select[aria-label="Bedrooms"]', '2');
  // ...
});
```

---

### Phase 5: Fix Admin Tests (Priority: MEDIUM)
**Files to Update**:
1. `tests/RentalManager.E2ETests/tests/property-application-flow.spec.ts`
   - Replace admin email/password login with authenticatedPage fixture
   - Create admin-specific fixture if needed

**Changes**:
```typescript
// Create admin fixture or use authenticatedPage with admin role
test.beforeEach(async ({ page }) => {
  await page.goto('/');
  await page.evaluate(() => {
    localStorage.setItem('auth_token', mockAdminToken);
    localStorage.setItem('user', JSON.stringify({
      ...mockUser,
      role: 'Admin',
      email: 'admin@example.com'
    }));
  });
});
```

---

## Testing Strategy

After fixes:
1. Run full test suite: `npm test`
2. Verify each fixed test passes individually
3. Check that existing passing tests still pass
4. Review test coverage to ensure no regressions

---

## Estimated Effort

- **Phase 1** (Navigation): ~30 minutes
- **Phase 2** (Authentication): ~45 minutes  
- **Phase 3** (Property Details): ~15 minutes
- **Phase 4** (Filters): ~15 minutes
- **Phase 5** (Admin): ~30 minutes

**Total**: ~2.25 hours

---

## Notes

- The `authenticatedPage` fixture already exists and sets localStorage correctly
- MainNavigation uses React Router Links, not buttons
- Filter button is mobile-only by design (`lg:hidden`)
- PropertyDetailPage shows "Apply Now" only when `property.status === 0`

