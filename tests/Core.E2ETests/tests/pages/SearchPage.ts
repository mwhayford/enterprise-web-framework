// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

import { Page } from '@playwright/test';
import { BasePage } from './BasePage';

/**
 * Page Object Model for the Search page.
 */
export class SearchPage extends BasePage {
  private readonly searchInput = 'input[type="search"]';
  private readonly searchButton = 'button:has-text("Search")';
  private readonly indexSelect = 'select[aria-label="Search index"]';
  private readonly resultsContainer = '[class*="results"]';
  private readonly noResultsMessage = 'text=No results found';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to the search page.
   */
  async navigate(): Promise<void> {
    await this.goto('/search');
    await this.waitForLoad();
  }

  /**
   * Perform a search with a query and index.
   */
  async search(query: string, index: string = 'all'): Promise<void> {
    await this.page.selectOption(this.indexSelect, index);
    await this.page.fill(this.searchInput, query);
    await this.page.click(this.searchButton);
    await this.page.waitForTimeout(1000); // Wait for results
  }

  /**
   * Check if results are displayed.
   */
  async hasResults(): Promise<boolean> {
    return await this.isVisible(this.resultsContainer);
  }

  /**
   * Check if "no results" message is displayed.
   */
  async hasNoResults(): Promise<boolean> {
    return await this.isVisible(this.noResultsMessage);
  }

  /**
   * Get the number of results displayed.
   */
  async getResultCount(): Promise<number> {
    const results = await this.page.locator('[class*="result-item"]').count();
    return results;
  }
}

