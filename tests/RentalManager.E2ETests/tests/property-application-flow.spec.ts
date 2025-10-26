import { test, expect } from '@playwright/test';

test.describe('Property Application Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should allow browsing available properties', async ({ page }) => {
    // Navigate to properties page
    await page.click('text=Properties');
    await expect(page).toHaveURL('/properties');

    // Check for property listings
    await expect(page.locator('[data-testid="property-card"]').first()).toBeVisible();
  });

  test('should show property details', async ({ page }) => {
    // Navigate to properties page
    await page.goto('/properties');

    // Click on first property
    await page.locator('[data-testid="property-card"]').first().click();

    // Check for property details
    await expect(page.locator('text=Property Details')).toBeVisible();
    await expect(page.locator('text=Apply Now')).toBeVisible();
  });

  test('should require authentication for application submission', async ({ page }) => {
    // Navigate to a property
    await page.goto('/properties');
    await page.locator('[data-testid="property-card"]').first().click();

    // Click Apply Now without logging in
    await page.click('text=Apply Now');

    // Should redirect to login
    await expect(page).toHaveURL(/\/login/);
  });

  test('authenticated user can submit application', async ({ page }) => {
    // Login first (assumes test user exists)
    await page.goto('/login');
    await page.fill('input[name="email"]', 'test@example.com');
    await page.fill('input[name="password"]', 'Test123!');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL('/dashboard');

    // Navigate to property and apply
    await page.goto('/properties');
    await page.locator('[data-testid="property-card"]').first().click();
    await page.click('text=Apply Now');

    // Fill out application form - Step 1: Personal Info
    await page.fill('input[aria-label="First name"]', 'John');
    await page.fill('input[aria-label="Last name"]', 'Doe');
    await page.fill('input[aria-label="Email"]', 'john.doe@example.com');
    await page.fill('input[aria-label="Phone"]', '555-0100');
    await page.fill('input[type="date"]', '1990-01-01');
    await page.click('text=Next');

    // Step 2: Employment Info
    await page.fill('input[aria-label="Employer name"]', 'Tech Corp');
    await page.fill('input[aria-label="Job title"]', 'Software Engineer');
    await page.fill('input[aria-label="Annual income"]', '75000');
    await page.fill('input[aria-label="Years employed"]', '3');
    await page.click('text=Next');

    // Step 3: Rental History
    await page.click('text=Next');

    // Step 4: References and submission
    await page.check('input[type="checkbox"]');
    await page.click('text=Submit Application');

    // Check for success message or redirect
    await expect(page.locator('text=Application submitted successfully')).toBeVisible();
  });

  test('should filter properties by criteria', async ({ page }) => {
    await page.goto('/properties');

    // Open filters
    await page.click('[aria-label="Open filters"]');

    // Set bedrooms filter
    await page.selectOption('select[aria-label="Bedrooms"]', '2');

    // Apply filters
    await page.click('text=Apply Filters');

    // Check that filtered results are shown
    await expect(page.locator('[data-testid="property-card"]')).toHaveCount(1, { timeout: 5000 });
  });

  test('should view submitted applications', async ({ page }) => {
    // Login first
    await page.goto('/login');
    await page.fill('input[name="email"]', 'test@example.com');
    await page.fill('input[name="password"]', 'Test123!');
    await page.click('button[type="submit"]');

    // Navigate to My Applications
    await page.goto('/applications/my');

    // Check for applications list
    await expect(page.locator('text=My Applications')).toBeVisible();
  });
});

test.describe('Admin Application Management', () => {
  test.beforeEach(async ({ page }) => {
    // Login as admin
    await page.goto('/login');
    await page.fill('input[name="email"]', 'admin@example.com');
    await page.fill('input[name="password"]', 'Admin123!');
    await page.click('button[type="submit"]');
  });

  test('admin can view all applications', async ({ page }) => {
    await page.goto('/admin/applications');

    await expect(page.locator('text=Application Management')).toBeVisible();
    await expect(page.locator('table')).toBeVisible();
  });

  test('admin can review and approve application', async ({ page }) => {
    await page.goto('/admin/applications');

    // Click on first application to review
    await page.locator('text=Review').first().click();

    // Add decision notes
    await page.fill('textarea[id="decisionNotes"]', 'Strong application, approved');

    // Approve application
    await page.click('text=Approve Application');

    // Check for success message
    await expect(page).toHaveURL('/admin/applications');
  });

  test('admin can review and reject application', async ({ page }) => {
    await page.goto('/admin/applications');

    // Click on first application to review
    await page.locator('text=Review').first().click();

    // Add decision notes
    await page.fill('textarea[id="decisionNotes"]', 'Insufficient documentation');

    // Reject application
    await page.click('text=Reject Application');

    // Check for success message
    await expect(page).toHaveURL('/admin/applications');
  });

  test('admin can filter applications by status', async ({ page }) => {
    await page.goto('/admin/applications');

    // Click on filter button for "Approved"
    await page.click('text=Approved');

    // Check that only approved applications are shown
    await expect(page.locator('text=Approved').first()).toBeVisible();
  });
});

