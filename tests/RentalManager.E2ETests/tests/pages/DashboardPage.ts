// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

import { Page } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object Model for the Dashboard page.
 */
export class DashboardPage extends BasePage {
  private readonly welcomeMessage = 'text=Welcome';
  // Navigation links from MainNavigation component
  private readonly paymentLink = 'nav a[href="/payment"]';
  private readonly subscriptionLink = 'nav a[href="/subscription"]';
  private readonly paymentMethodsLink = 'nav a[href="/payment-methods"]';
  private readonly searchLink = 'nav a[href="/search"]';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to the dashboard page.
   */
  async navigate(): Promise<void> {
    await this.goto('/dashboard');
    await this.waitForLoad();
  }

  /**
   * Check if the dashboard is loaded.
   */
  async isDashboardLoaded(): Promise<boolean> {
    return await this.isVisible(this.welcomeMessage);
  }

  /**
   * Navigate to the payment page.
   * Uses navigation link if available, otherwise navigates directly.
   */
  async goToPayment(): Promise<void> {
    // Payment is in navigation, try clicking the link
    const paymentNavLink = this.page.locator(this.paymentLink);
    if (await paymentNavLink.isVisible({ timeout: 2000 }).catch(() => false)) {
      await paymentNavLink.click();
    } else {
      // Fallback: navigate directly
      await this.goto('/payment');
    }
    await this.page.waitForURL('**/payment');
  }

  /**
   * Navigate to the subscription page.
   * Note: Subscription is not in main navigation, so navigate directly.
   */
  async goToSubscription(): Promise<void> {
    // Subscription is not in navigation, navigate directly
    await this.goto('/subscription');
    await this.page.waitForURL('**/subscription');
  }

  /**
   * Navigate to the payment methods page.
   * Note: Payment Methods is not in main navigation, so navigate directly.
   */
  async goToPaymentMethods(): Promise<void> {
    // Payment Methods is not in navigation, navigate directly
    await this.goto('/payment-methods');
    await this.page.waitForURL('**/payment-methods');
  }

  /**
   * Navigate to the search page.
   * Uses navigation link if available, otherwise navigates directly.
   */
  async goToSearch(): Promise<void> {
    // Search is in navigation, try clicking the link
    const searchNavLink = this.page.locator(this.searchLink);
    if (await searchNavLink.isVisible({ timeout: 2000 }).catch(() => false)) {
      await searchNavLink.click();
    } else {
      // Fallback: navigate directly
      await this.goto('/search');
    }
    await this.page.waitForURL('**/search');
  }
}

