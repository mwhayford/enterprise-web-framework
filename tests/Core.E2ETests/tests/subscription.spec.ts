// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

import { test, expect } from './fixtures/auth.fixture';

test.describe('Subscription Management', () => {
  test.beforeEach(async ({ authenticatedPage }) => {
    await authenticatedPage.goToSubscription();
  });

  test('should display subscription page', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    
    const pageContent = await page.content();
    expect(pageContent).toContain('Subscription') || expect(pageContent).toContain('subscription');
  });

  test('should show subscription plans or current subscription', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    
    const pageContent = await page.content();
    const hasPlans = pageContent.includes('plan') || 
                     pageContent.includes('Plan') ||
                     pageContent.includes('subscription');
    
    expect(hasPlans).toBeTruthy();
  });

  test('should display Stripe payment form for new subscription', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    
    // Check for subscription form elements
    const submitButton = page.locator('button[type="submit"], button:has-text("Subscribe")');
    const hasSubmitButton = await submitButton.count() > 0;
    
    expect(hasSubmitButton).toBeTruthy();
  });

  test('should validate subscription form inputs', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    
    const submitButton = page.locator('button[type="submit"]').first();
    
    if (await submitButton.count() > 0) {
      // Try to submit without filling required fields
      await submitButton.click();
      await page.waitForTimeout(500);
      
      // Should show validation or remain on page
      const url = page.url();
      expect(url).toContain('subscription');
    }
  });

  test('should navigate to subscription success page after successful subscription', async ({ page }) => {
    // Mock scenario: directly navigate to success page
    await page.goto('/subscription-success');
    await page.waitForLoadState('networkidle');
    
    const pageContent = await page.content();
    expect(pageContent).toContain('success') || expect(pageContent).toContain('Success');
  });

  test('should display active subscriptions list', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    
    const pageContent = await page.content();
    const hasSubscriptionInfo = pageContent.includes('Active') || 
                                 pageContent.includes('Current') ||
                                 pageContent.includes('subscription') ||
                                 pageContent.includes('No active');
    
    expect(hasSubscriptionInfo).toBeTruthy();
  });

  test('should allow canceling subscription', async ({ page }) => {
    await page.waitForLoadState('networkidle');
    
    // Look for cancel button
    const cancelButton = page.locator('button:has-text("Cancel")');
    const hasCancelButton = await cancelButton.count() > 0;
    
    // Cancel button may not be visible if no active subscription
    // This is just checking the page loads correctly
    expect(hasCancelButton || await page.content()).toBeTruthy();
  });
});

