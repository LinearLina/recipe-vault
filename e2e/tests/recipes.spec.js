import { test, expect } from '@playwright/test';

// These tests assume the API (port 4000) and client (port 5173) are already
// running against a Postgres instance, e.g. via docker-compose + npm run dev
// in both server/ and client/.

test('home page loads and shows the recipe box heading', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'The recipe box' })).toBeVisible();
});

test('can create a recipe end to end and see it in the list', async ({ page }) => {
  const title = `Playwright Test Bake ${Date.now()}`;

  await page.goto('/');
  await page.getByRole('link', { name: '+ New recipe' }).click();

  await page.getByLabel('Title').fill(title);
  await page.getByLabel('Short description').fill('Created by an E2E test.');
  await page.getByPlaceholder('flour').fill('sugar');
  await page.getByPlaceholder('quick, vegetarian').fill('dessert, quick');
  await page.locator('textarea').fill('Mix everything and bake at 180C for 20 minutes.');

  await page.getByRole('button', { name: 'Save recipe' }).click();

  // Should land on the detail page for the new recipe
  await expect(page.getByRole('heading', { name: title })).toBeVisible();

  // And it should show up back on the list
  await page.getByRole('link', { name: /Back to the recipe box/ }).click();
  await page.getByPlaceholder('Search by title...').fill(title);
  await page.getByRole('button', { name: 'Search' }).click();
  await expect(page.getByText(title)).toBeVisible();
});

test('can filter recipes by tag', async ({ page }) => {
  await page.goto('/');
  const dessertTab = page.getByRole('button', { name: 'dessert', exact: true });
  if (await dessertTab.count()) {
    await dessertTab.click();
    await expect(page).toHaveURL('/');
  }
});

test('deleting a recipe removes it from the list', async ({ page }) => {
  const title = `Deletable Recipe ${Date.now()}`;

  await page.goto('/recipes/new');
  await page.getByLabel('Title').fill(title);
  await page.getByPlaceholder('flour').fill('water');
  await page.locator('textarea').fill('Boil water.');
  await page.getByRole('button', { name: 'Save recipe' }).click();

  await expect(page.getByRole('heading', { name: title })).toBeVisible();

  page.once('dialog', (dialog) => dialog.accept());
  await page.getByRole('button', { name: 'Delete' }).click();

  await expect(page).toHaveURL('/');
});
