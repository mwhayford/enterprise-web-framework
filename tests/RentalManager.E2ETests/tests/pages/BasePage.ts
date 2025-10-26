// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

import { Page } from '@playwright/test';

/**
 * Base page class with common functionality for all page objects.
 */
export class BasePage {
  constructor(protected page: Page) {}

  /**
   * Navigate to a specific path.
   */
  async goto(path: string): Promise<void> {
    await this.page.goto(path);
  }

  /**
   * Wait for page to load completely.
   */
  async waitForLoad(): Promise<void> {
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Get the current URL.
   */
  getUrl(): string {
    return this.page.url();
  }

  /**
   * Check if element is visible.
   */
  async isVisible(selector: string): Promise<boolean> {
    return await this.page.isVisible(selector);
  }

  /**
   * Take a screenshot.
   */
  async screenshot(name: string): Promise<void> {
    await this.page.screenshot({ path: `test-results/screenshots/${name}.png` });
  }
}

