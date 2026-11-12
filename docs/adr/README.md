# Architecture Decision Records

Short notes on decisions that shaped this codebase: what was decided, what else was
considered, and what it costs. New decisions get the next number, using the template below.

| # | Decision | Status |
|---|----------|--------|
| [0001](0001-migrate-to-dotnet-and-sql-server.md) | Rebuild the API in ASP.NET Core with SQL Server | Accepted |
| [0002](0002-layered-architecture.md) | Layered architecture with EF Core in the application layer | Accepted |
| [0003](0003-integration-tests-against-real-sql-server.md) | Integration tests run against a real SQL Server | Accepted |
| [0004](0004-keep-the-existing-api-contract.md) | Keep the Express API's JSON contract during the migration | Accepted |
| [0005](0005-results-for-expected-failures.md) | Return results for expected failures, throw only for bugs | Accepted |
| [0006](0006-authentication-and-ownership.md) | JWT authentication and recipe ownership | Accepted |

## Template

```markdown
# NNNN: Title

- Status: Proposed | Accepted | Superseded by NNNN
- Date: YYYY-MM-DD

## Context
What problem or constraint forced a decision?

## Decision
What we chose.

## Alternatives considered
What else, and why not.

## Consequences
What gets easier, what gets harder, and any debt this creates.
```
