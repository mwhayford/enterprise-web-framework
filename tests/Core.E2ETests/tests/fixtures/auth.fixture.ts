// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

import { test as base } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { DashboardPage } from '../pages/DashboardPage';

/**
 * Authentication fixture that provides authenticated context.
 * This fixture can be extended to handle OAuth mocking or test user setup.
 */
type AuthFixtures = {
  loginPage: LoginPage;
  dashboardPage: DashboardPage;
  authenticatedPage: DashboardPage;
};

export const test = base.extend<AuthFixtures>({
  loginPage: async ({ page }, use) => {
    const loginPage = new LoginPage(page);
    await use(loginPage);
  },

  dashboardPage: async ({ page }, use) => {
    const dashboardPage = new DashboardPage(page);
    await use(dashboardPage);
  },

  /**
   * Provides an authenticated page context.
   * In a real scenario, this would:
   * 1. Mock OAuth responses
   * 2. Set JWT tokens in localStorage
   * 3. Or use a test user account
   */
  authenticatedPage: async ({ page }, use) => {
    // Mock authentication by setting localStorage
    await page.goto('/');
    await page.evaluate(() => {
      const mockToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ0ZXN0LXVzZXItaWQiLCJuYW1lIjoiVGVzdCBVc2VyIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c';
      const mockUser = {
        id: 'test-user-id',
        email: 'test@example.com',
        firstName: 'Test',
        lastName: 'User',
      };
      localStorage.setItem('token', mockToken);
      localStorage.setItem('user', JSON.stringify(mockUser));
    });

    const dashboardPage = new DashboardPage(page);
    await dashboardPage.navigate();
    await use(dashboardPage);
  },
});

export { expect } from '@playwright/test';

