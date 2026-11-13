# Working on Recipe Vault

Guidance for AI coding assistants (and humans) working in this repo. These are the rules
I review AI-generated changes against.

## Architecture

- `backend/` is the ASP.NET Core API. Dependencies point inwards: Api → Application → Domain,
  Infrastructure → Application. Never reference Infrastructure or Api from Application or Domain.
- Business rules belong on the Domain entities (e.g. `Recipe.ScaleIngredients`). Controllers stay thin:
  call one service method, map the `Result` with `Failure(...)`.
- Expected failures are returned as `Result<T>`, never thrown (ADR 0005).
- The JSON contract (snake_case, `{ error }` / `{ errors: [] }`) must not change without an ADR,
  because the React client depends on it (ADR 0004).

## Conventions

- C#: file-scoped namespaces, primary constructors for services, `async` all the way with a
  `CancellationToken`, records for response DTOs, nullable reference types on.
- No new NuGet packages without an ADR explaining why. Versions live in `Directory.Packages.props`.
- No suppressing warnings; the build treats warnings as errors.
- Database changes go through an EF Core migration:
  `dotnet ef migrations add <Name> --project src/RecipeVault.Infrastructure --startup-project src/RecipeVault.Api --output-dir Persistence/Migrations`

## Testing

- New domain rule → unit test in `RecipeVault.UnitTests`.
- New or changed endpoint → integration tests for the success case, validation failure,
  not found, and (for writes) not logged in / not the owner.
- Integration tests use real SQL Server. Do not switch them to the in-memory provider or SQLite (ADR 0003).
- User-visible behaviour change → update or add a Playwright test in `e2e/tests`.

## Before you say a change is done

1. `dotnet build` with no warnings.
2. `dotnet test` passes.
3. `dotnet format whitespace --verify-no-changes` passes.
4. If you took a shortcut, add it to `docs/tech-debt.md`.
