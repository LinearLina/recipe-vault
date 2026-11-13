# 0003: Integration tests run against a real SQL Server

- Status: Accepted
- Date: 2026-11-12

## Context

The API tests need a database. EF Core offers an in-memory provider and SQLite as quick options.

## Decision

Integration tests host the real API with `WebApplicationFactory` and run against real SQL Server:
LocalDB on a developer machine, the SQL Server service container in CI. The schema is created with
the real EF migrations, and [Respawn](https://github.com/jbogard/Respawn) empties the tables
before each test so every test starts clean.

## Alternatives considered

- **EF Core in-memory provider.** Not a relational database: no foreign keys, no unique indexes,
  no transactions, different query behaviour. Tests can pass while production fails.
- **SQLite.** Closer, but still behaves differently. When checking this code, a SQLite run failed
  two tests that pass on SQL Server: SQLite can't `ORDER BY` a `DateTimeOffset`, and it compares tag
  names case-sensitively. That's precisely the kind of difference that makes tests untrustworthy.

## Consequences

- Tests catch real database problems (constraint violations, migration mistakes, collation).
- Running integration tests locally needs LocalDB (Windows) or a test database set in
  `RECIPEVAULT_TEST_DB`. The test fixture refuses to run unless the database name contains "test",
  because Respawn deletes every row.
- Slower than in-memory, but still a few seconds for the whole suite.
