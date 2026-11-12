# 0002: Layered architecture with EF Core in the application layer

- Status: Accepted
- Date: 2026-11-12

## Context

The Express API kept SQL, validation and HTTP handling in the same route functions. That made
rules (like "servings must be at least 1") hard to test without a database and easy to miss in
one route but not another (e.g. `PUT` skipped ingredients and tags entirely).

## Decision

Four projects, with dependencies pointing inwards:

```
Api ──► Application ──► Domain
 │           ▲
 └─► Infrastructure
```

- **Domain**: entities and their rules (`Recipe.ScaleIngredients`, tag de-duplication). No dependencies.
- **Application**: use cases (`RecipeService`, `AuthService`), request validation, DTOs.
- **Infrastructure**: EF Core `DbContext`, table configuration, migrations, password hashing.
- **Api**: controllers, authentication, error responses. Kept thin.

The application layer talks to the database through `IAppDbContext`, which exposes EF Core
`DbSet`s, rather than through a custom repository per entity.

## Alternatives considered

- **Repository pattern** (`IRecipeRepository` etc.). Fully hides EF Core from the application layer,
  but for a CRUD-heavy app most repository methods would just pass through to EF, adding code
  without adding much. `DbSet` already is a repository and unit of work.
- **Single project.** Simpler, but the domain rules end up mixed with HTTP and SQL again.

## Consequences

- Domain rules are covered by fast unit tests with no database.
- The application layer references `Microsoft.EntityFrameworkCore`. That ties it to EF Core (not to
  SQL Server), so switching ORM would mean changing the services. I accept that trade-off.
- Provider-specific settings (query splitting, retry on failure) live in Infrastructure, so the
  services stay provider-agnostic.
