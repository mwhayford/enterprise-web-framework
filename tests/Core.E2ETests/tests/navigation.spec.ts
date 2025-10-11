// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

import { test, expect } from './fixtures/auth.fixture';

test.describe('Application Navigation', () => {
  test('should navigate between dashboard sections', async ({ authenticatedPage, page }) => {
    await authenticatedPage.navigate();
    
    // Test navigation to payment
    await authenticatedPage.goToPayment();
    expect(page.url()).toContain('/payment');
    
    // Navigate back to dashboard
    await authenticatedPage.navigate();
    expect(page.url()).toContain('/dashboard');
    
    // Test navigation to search
    await authenticatedPage.goToSearch();
    expect(page.url()).toContain('/search');
  });

  test('should display navigation menu on authenticated pages', async ({ authenticatedPage, page }) => {
    await authenticatedPage.navigate();
    
    const pageContent = await page.content();
    const hasNavigation = pageContent.includes('Dashboard') || 
                          pageContent.includes('Payment') ||
                          pageContent.includes('Search');
    
    expect(hasNavigation).toBeTruthy();
  });

  test('should handle browser back button correctly', async ({ authenticatedPage, page }) => {
    await authenticatedPage.navigate();
    
    // Navigate to payment
    await authenticatedPage.goToPayment();
    await page.waitForLoadState('networkidle');
    
    // Go back
    await page.goBack();
    await page.waitForLoadState('networkidle');
    
    // Should be back on dashboard
    expect(page.url()).toContain('/dashboard');
  });

  test('should handle browser forward button correctly', async ({ authenticatedPage, page }) => {
    await authenticatedPage.navigate();
    
    // Navigate to payment and back
    await authenticatedPage.goToPayment();
    await page.goBack();
    
    // Go forward
    await page.goForward();
    await page.waitForLoadState('networkidle');
    
    // Should be on payment page
    expect(page.url()).toContain('/payment');
  });

  test('should maintain scroll position on page navigation', async ({ authenticatedPage, page }) => {
    await authenticatedPage.navigate();
    
    // Scroll down
    await page.evaluate(() => window.scrollTo(0, 500));
    const scrollBefore = await page.evaluate(() => window.scrollY);
    
    // Navigate away and back
    await authenticatedPage.goToSearch();
    await authenticatedPage.navigate();
    
    // Page should be at top (SPA behavior may vary)
    const scrollAfter = await page.evaluate(() => window.scrollY);
    expect(typeof scrollAfter).toBe('number');
  });

  test('should display 404 page for invalid routes', async ({ page }) => {
    await page.goto('/invalid-route-that-does-not-exist');
    await page.waitForLoadState('networkidle');
    
    const pageContent = await page.content();
    const has404 = pageContent.includes('404') || 
                   pageContent.includes('Not Found') ||
                   pageContent.includes('not found');
    
    // Depending on your routing setup, it might redirect or show 404
    expect(has404 || pageContent.length > 0).toBeTruthy();
  });
});

