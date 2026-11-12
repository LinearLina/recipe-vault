#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
echo "==> Restoring .NET packages and tools"
(cd backend && dotnet restore && dotnet tool restore)
echo "==> Installing client and E2E dependencies"
npm ci --prefix client
npm ci --prefix e2e
echo "==> Creating the database and loading sample recipes"
for attempt in $(seq 1 10); do
  if dotnet run --project backend/src/RecipeVault.Api --no-launch-profile -- --migrate --seed; then
    echo "==> Ready."
    exit 0
  fi
  echo "SQL Server not ready yet (attempt $attempt/10), retrying in 10s..."
  sleep 10
done
echo "Could not create the database."
exit 1
