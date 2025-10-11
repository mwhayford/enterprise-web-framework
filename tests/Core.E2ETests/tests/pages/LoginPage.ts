// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

import { Page } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object Model for the Login page.
 */
export class LoginPage extends BasePage {
  private readonly googleLoginButton = 'button:has-text("Sign in with Google")';
  private readonly emailInput = 'input[type="email"]';
  private readonly passwordInput = 'input[type="password"]';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to the login page.
   */
  async navigate(): Promise<void> {
    await this.goto('/login');
    await this.waitForLoad();
  }

  /**
   * Check if the Google login button is visible.
   */
  async isGoogleLoginVisible(): Promise<boolean> {
    return await this.isVisible(this.googleLoginButton);
  }

  /**
   * Click the Google login button.
   * Note: In E2E tests, we typically mock OAuth or use test credentials.
   */
  async clickGoogleLogin(): Promise<void> {
    await this.page.click(this.googleLoginButton);
  }

  /**
   * Wait for redirect after login.
   */
  async waitForLoginRedirect(): Promise<void> {
    await this.page.waitForURL('**/dashboard', { timeout: 10000 });
  }
}

