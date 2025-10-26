// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

import { Page } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object Model for the Dashboard page.
 */
export class DashboardPage extends BasePage {
  private readonly welcomeMessage = 'text=Welcome';
  private readonly paymentLink = 'button:has-text("Make Payment")';
  private readonly subscriptionLink = 'button:has-text("Subscribe")';
  private readonly paymentMethodsLink = 'button:has-text("Payment Methods")';
  private readonly searchLink = 'button:has-text("Search Content")';

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
   */
  async goToPayment(): Promise<void> {
    await this.page.click(this.paymentLink);
    await this.page.waitForURL('**/payment');
  }

  /**
   * Navigate to the subscription page.
   */
  async goToSubscription(): Promise<void> {
    await this.page.click(this.subscriptionLink);
    await this.page.waitForURL('**/subscription');
  }

  /**
   * Navigate to the payment methods page.
   */
  async goToPaymentMethods(): Promise<void> {
    await this.page.click(this.paymentMethodsLink);
    await this.page.waitForURL('**/payment-methods');
  }

  /**
   * Navigate to the search page.
   */
  async goToSearch(): Promise<void> {
    await this.page.click(this.searchLink);
    await this.page.waitForURL('**/search');
  }
}

