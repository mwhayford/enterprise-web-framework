// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

import { test, expect } from './fixtures/auth.fixture';

test.describe('Authentication Flow', () => {
  test('should display login page with Google sign-in button', async ({ loginPage }) => {
    await loginPage.navigate();
    const isGoogleLoginVisible = await loginPage.isGoogleLoginVisible();
    expect(isGoogleLoginVisible).toBeTruthy();
  });

  test('should redirect to dashboard after successful login', async ({ authenticatedPage }) => {
    const isDashboardLoaded = await authenticatedPage.isDashboardLoaded();
    expect(isDashboardLoaded).toBeTruthy();
    expect(authenticatedPage.getUrl()).toContain('/dashboard');
  });

  test('should display user information on dashboard', async ({ authenticatedPage, page }) => {
    await authenticatedPage.navigate();
    const pageContent = await page.content();
    expect(pageContent).toContain('Welcome');
  });

  test('should maintain authentication across page navigation', async ({ authenticatedPage, page }) => {
    await authenticatedPage.navigate();
    
    // Navigate to another page
    await authenticatedPage.goToSearch();
    await page.waitForLoadState('networkidle');
    
    // Check if still authenticated (token exists)
    const token = await page.evaluate(() => localStorage.getItem('token'));
    expect(token).toBeTruthy();
  });

  test('should redirect to login when accessing protected route without auth', async ({ page }) => {
    // Clear any existing auth
    await page.goto('/');
    await page.evaluate(() => {
      localStorage.clear();
    });

    // Try to access dashboard
    await page.goto('/dashboard');
    await page.waitForTimeout(1000);

    // Should be redirected to login (depending on your auth implementation)
    const url = page.url();
    expect(url).toMatch(/\/(login|$)/);
  });
});

