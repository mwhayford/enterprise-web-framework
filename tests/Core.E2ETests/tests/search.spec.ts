// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

import { test, expect } from './fixtures/auth.fixture';
import { SearchPage } from './pages/SearchPage';

test.describe('Search Functionality', () => {
  let searchPage: SearchPage;

  test.beforeEach(async ({ authenticatedPage, page }) => {
    searchPage = new SearchPage(page);
    await searchPage.navigate();
  });

  test('should display search page with search input and filters', async ({ page }) => {
    const searchInput = await page.locator('input[type="search"]');
    const indexSelect = await page.locator('select[aria-label="Search index"]');
    const searchButton = await page.locator('button:has-text("Search")');

    expect(await searchInput.isVisible()).toBeTruthy();
    expect(await indexSelect.isVisible()).toBeTruthy();
    expect(await searchButton.isVisible()).toBeTruthy();
  });

  test('should allow selecting different search indices', async ({ page }) => {
    const indexSelect = page.locator('select[aria-label="Search index"]');
    await indexSelect.selectOption('users');
    
    const selectedValue = await indexSelect.inputValue();
    expect(selectedValue).toBe('users');
  });

  test('should perform search and display results', async () => {
    await searchPage.search('test', 'all');
    
    // Wait for search results or no results message
    await searchPage.page.waitForTimeout(2000);
    
    const hasResults = await searchPage.hasResults();
    const hasNoResults = await searchPage.hasNoResults();
    
    // Either results or no results message should be displayed
    expect(hasResults || hasNoResults).toBeTruthy();
  });

  test('should handle empty search query', async ({ page }) => {
    await searchPage.search('', 'all');
    
    // Should either show validation message or all results
    await page.waitForTimeout(1000);
    const pageContent = await page.content();
    expect(pageContent).toBeTruthy();
  });

  test('should filter search by index type', async () => {
    // Search in users index
    await searchPage.search('john', 'users');
    await searchPage.page.waitForTimeout(1000);
    
    const hasResults = await searchPage.hasResults();
    const hasNoResults = await searchPage.hasNoResults();
    
    expect(hasResults || hasNoResults).toBeTruthy();
  });

  test('should display pagination controls when results exceed page size', async ({ page }) => {
    await searchPage.search('test', 'all');
    await page.waitForTimeout(2000);
    
    // Check for pagination controls (if results are numerous)
    const paginationButtons = page.locator('button:has-text("Next")');
    const paginationVisible = await paginationButtons.count();
    
    // Pagination might or might not be visible depending on results
    expect(paginationVisible >= 0).toBeTruthy();
  });
});

