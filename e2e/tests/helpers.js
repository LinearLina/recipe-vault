import { expect } from '@playwright/test';

export const API_URL = process.env.API_URL || 'http://localhost:4000/api';

/**
 * Creates a brand-new user through the API and puts their token in localStorage,
 * so the test starts already logged in. Faster and less flaky than clicking through
 * the login form in every test (login itself has its own UI test in auth.spec.js).
 */
export async function logInAsNewUser(page, request) {
  const email = `e2e-${Date.now()}-${Math.random().toString(36).slice(2, 8)}@example.com`;
  const password = 'e2e-password-123';

  const signup = await request.post(`${API_URL}/signup`, { data: { email, password } });
  expect(signup.status()).toBe(201);

  const login = await request.post(`${API_URL}/login`, { data: { email, password } });
  expect(login.ok()).toBeTruthy();
  const { token } = await login.json();

  await page.addInitScript((t) => window.localStorage.setItem('recipe_vault_token', t), token);
  return { email, password };
}
