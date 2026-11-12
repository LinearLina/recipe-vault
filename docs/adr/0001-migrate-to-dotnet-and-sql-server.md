# 0001: Rebuild the API in ASP.NET Core with SQL Server

- Status: Accepted
- Date: 2026-11-12

## Context

Recipe Vault started as an Express + PostgreSQL API. I want the project to show
C#/.NET and SQL Server skills, which are the stack used in the roles I'm applying for.
My laptop can't run Docker, so the local database can't depend on containers.

## Decision

- Rewrite the API in ASP.NET Core (.NET 10, the current LTS release) with EF Core and SQL Server.
- Keep the React client unchanged for now; the new API matches the old one's routes and JSON
  (see [0004](0004-keep-the-existing-api-contract.md)), so the client and the Playwright tests
  prove the two are equivalent.
- Local database: SQL Server LocalDB on Windows (no Docker), or an Azure SQL free-tier database.
  CI uses the official SQL Server container, which GitHub's runners can run.

## Alternatives considered

- **Keep PostgreSQL with EF Core (Npgsql).** Less migration work, but doesn't show SQL Server.
- **Migrate the client to Angular at the same time.** Too much change at once; if something broke
  it would be hard to tell which side caused it. Planned as a separate step.

## Consequences

- One API to maintain; the Express server is removed once parity is proven.
- EF Core migrations replace the hand-written SQL file; CI fails if the model and migrations drift.
- Postgres-only features were dropped: `ILIKE` became `Contains` (SQL Server's default collation
  is already case-insensitive) and the GIN full-text index became a normal index on `Title`.
