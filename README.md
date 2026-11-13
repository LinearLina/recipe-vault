# Recipe Vault

[![CI](https://github.com/LinearLina/recipe-vault/actions/workflows/ci.yml/badge.svg)](https://github.com/LinearLina/recipe-vault/actions/workflows/ci.yml)

A full-stack recipe organiser: an **ASP.NET Core (.NET 10) API with EF Core and SQL Server**,
a React (Vite) frontend, and automated tests at three levels (unit, integration against a real
SQL Server, and Playwright end-to-end), all run in GitHub Actions on every push.

The API was originally Node/Express + PostgreSQL. I rebuilt it in C# while keeping the same
routes and JSON, so the existing React app and E2E tests prove the new API is a drop-in
replacement. Porting it also surfaced several bugs in the original. They're listed, with the
regression tests that cover them, in [docs/tech-debt.md](docs/tech-debt.md#fixed-during-the-net-migration).

## Highlights

- **Layered architecture**: Domain → Application → Infrastructure → Api, with business rules on the
  entities and thin controllers ([ADR 0002](docs/adr/0002-layered-architecture.md)).
- **Relational design with EF Core**: recipes → ingredients (ordered, one-to-many), recipes ↔ tags
  (many-to-many), users → recipes, cascade deletes, unique indexes, code-first migrations.
- **Auth**: signup/login with hashed passwords and JWT bearer tokens; only a recipe's owner can change it.
- **Testing pyramid**: xUnit unit tests for domain rules and validation, integration tests that host the
  real API against SQL Server ([ADR 0003](docs/adr/0003-integration-tests-against-real-sql-server.md)),
  and Playwright tests that drive the UI.
- **CI quality gates**: warnings as errors, formatting check, a check that migrations match the model,
  tests with coverage, then E2E, with SQL Server running as a service container.
- **Decisions and debt are written down**: [Architecture Decision Records](docs/adr/README.md) and a
  [tech-debt log](docs/tech-debt.md).

## Stack

| Layer      | Tech |
|------------|------|
| API        | ASP.NET Core (.NET 10, LTS), C# |
| Data       | EF Core 10, SQL Server (LocalDB locally, Azure SQL or container elsewhere) |
| Auth       | JWT bearer tokens, ASP.NET Core Identity password hasher |
| Frontend   | React 18, Vite, plain CSS |
| Testing    | xUnit, WebApplicationFactory, Respawn, Playwright |
| CI         | GitHub Actions |

## Project structure

```
recipe-vault/
├── backend/
│   ├── RecipeVault.slnx
│   ├── src/
│   │   ├── RecipeVault.Domain/          # entities + business rules, no dependencies
│   │   ├── RecipeVault.Application/     # use cases, validation, DTOs
│   │   ├── RecipeVault.Infrastructure/  # EF Core DbContext, migrations, password hashing
│   │   └── RecipeVault.Api/             # controllers, auth, error handling, Program.cs
│   └── tests/
│       ├── RecipeVault.UnitTests/
│       └── RecipeVault.IntegrationTests/
├── client/                              # React (Vite) frontend
├── e2e/                                 # Playwright end-to-end tests
├── docs/
│   ├── adr/                             # architecture decision records
│   └── tech-debt.md
├── CLAUDE.md                            # conventions for AI-assisted changes
└── .github/workflows/ci.yml
```

## Running it locally

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) and Node.js 20+.
No Docker needed.

### 1. Database

**Windows:** SQL Server LocalDB comes with Visual Studio or the SQL Server Express installer.
The Development settings already point at `(localdb)\MSSQLLocalDB`, so there's nothing to configure.

**macOS / Linux:** create a free Azure SQL database and store its connection string as a user secret
(it stays out of git):

```bash
cd backend
dotnet user-secrets set "ConnectionStrings:Default" "<your Azure SQL connection string>" --project src/RecipeVault.Api
```

Then create the tables and load the sample recipes:

```bash
cd backend
dotnet run --project src/RecipeVault.Api -- --migrate --seed
```

### 2. API

```bash
cd backend
dotnet run --project src/RecipeVault.Api        # http://localhost:4000
```

`backend/src/RecipeVault.Api/RecipeVault.Api.http` has ready-made requests for VS Code or Visual Studio.
The OpenAPI document is at http://localhost:4000/openapi/v1.json in Development.

### 3. Frontend

```bash
cd client
npm install
npm run dev                                     # http://localhost:5173 (proxies /api to port 4000)
```

## Running the tests

```bash
cd backend
dotnet test tests/RecipeVault.UnitTests           # no database needed
dotnet test tests/RecipeVault.IntegrationTests    # needs LocalDB, or set RECIPEVAULT_TEST_DB

cd ../e2e
npm install && npx playwright install chromium
npm test                                          # starts the API and client if they aren't running
```

The integration tests use a separate `RecipeVault_Tests` database and empty it between tests.
To use another server, set `RECIPEVAULT_TEST_DB` to a connection string whose database name contains "test".

## API reference

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET    | /api/health | | `{ "status": "ok" }` when the database is reachable |
| GET    | /api/recipes | | List recipes, newest first. `?search=` (title) `&tag=` |
| GET    | /api/recipes/:id | | One recipe with ingredients (in order) and tags |
| GET    | /api/recipes/:id/scaled?servings=n | | Ingredient amounts recalculated for `n` servings |
| POST   | /api/recipes | ✓ | Create a recipe → `201 { id }` |
| PUT    | /api/recipes/:id | owner | Update a recipe, including ingredients and tags |
| DELETE | /api/recipes/:id | owner | Delete a recipe → `204` |
| GET    | /api/tags | | All tags, alphabetical |
| POST   | /api/signup | | `{ email, password }` → `201 { id, email }` |
| POST   | /api/login | | `{ email, password }` → `{ token }` |

Errors are `{ "error": "message" }`, or `{ "errors": ["message", ...] }` for validation
(see [ADR 0004](docs/adr/0004-keep-the-existing-api-contract.md)).

## How I used AI-assisted development

I used Claude to help port the API from Express to ASP.NET Core, write the tests and draft the docs (ADRs and the tech-debt log). I made the main decisions: keeping the existing JSON contract so the React client didn't need to change, and fixing the bugs we found in the Express code as part of the migration.

How I checked the AI's work
- Ran all the automated tests myself (47 unit and integration tests) and made sure CI passed
- Tested the app by hand, including each bug fix and the ownership rules, using two separate users to confirm one user can't delete another's recipe
- Set up and debugged the dev environment myself, including getting SQL Server running in GitHub Codespaces since it doesn't run on macOS

What I questioned or changed
- Spotted during testing that the Edit and Delete buttons show for recipes you don't own. The API blocks the action, but the UI shouldn't offer it, so I logged it as tech debt.
- When an E2E test failed in CI, I investigated and found a race condition in the test (it typed before the form had loaded), not a bug in the app

Keeping AI changes consistent
`CLAUDE.md` sets out the architecture rules, conventions and testing expectations I review AI-generated changes against, and the PR template asks how AI was used and checked on every change.

_Coming soon._

## What's next

- Port the client to **Angular + TypeScript**, then move errors to ProblemDetails
- Pagination on the recipe list
- Recipe costing and a costed shopping list
- A per-recipe assistant you can ask questions 

See [docs/tech-debt.md](docs/tech-debt.md) for the full list of known gaps.
