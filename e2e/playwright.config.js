import { defineConfig } from '@playwright/test';

const isCI = !!process.env.CI;

export default defineConfig({
  testDir: './tests',
  timeout: 30000,
  retries: isCI ? 1 : 0,
  reporter: isCI ? [['github'], ['html', { open: 'never' }]] : 'list',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
  },
  // Starts the API and the client for you. Locally, if you already have them running,
  // Playwright reuses them instead.
  webServer: [
    {
      command:
        'dotnet run --project ../backend/src/RecipeVault.Api --no-launch-profile --urls http://localhost:4000',
      url: 'http://localhost:4000/api/health',
      reuseExistingServer: !isCI,
      timeout: 180000,
      env: { ASPNETCORE_ENVIRONMENT: 'Development' },
    },
    {
      command: 'npm --prefix ../client run dev -- --port 5173 --strictPort',
      url: 'http://localhost:5173',
      reuseExistingServer: !isCI,
      timeout: 60000,
    },
  ],
});
