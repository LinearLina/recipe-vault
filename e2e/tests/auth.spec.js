import { test, expect } from '@playwright/test';

test('a new user can sign up and log in through the UI', async ({ page }) => {
  const email = `ui-${Date.now()}@example.com`;
  const password = 'ui-password-123';

  await page.goto('/signup');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Sign up' }).click();

  await expect(page).toHaveURL('/login');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: 'Log in' }).click();

  await expect(page.getByRole('button', { name: 'Log out' })).toBeVisible();
});

test('a wrong password shows an error instead of logging in', async ({ page }) => {
  await page.goto('/login');
  await page.getByLabel('Email').fill('nobody@example.com');
  await page.getByLabel('Password').fill('not-the-password');
  await page.getByRole('button', { name: 'Log in' }).click();

  await expect(page.getByText('Invalid email or password')).toBeVisible();
});
