# 0004: Keep the Express API's JSON contract during the migration

- Status: Accepted
- Date: 2026-11-12

## Context

ASP.NET Core's defaults differ from what the Express API returned:

| | Express | ASP.NET Core default |
|---|---|---|
| Property names | `prep_minutes` | `prepMinutes` |
| Validation errors | `{ "errors": ["title is required"] }` | ProblemDetails with `errors` as an object |
| Other errors | `{ "error": "Recipe not found" }` | ProblemDetails or an empty body |

The React client reads `data.errors.join(', ')`, so the default validation response would crash
it with a `TypeError`.

## Decision

Match the old contract exactly for now:

- `JsonNamingPolicy.SnakeCaseLower` for all controller JSON.
- A custom `InvalidModelStateResponseFactory` and a request validator that return `{ errors: [...] }`
  with the same messages as before.
- `{ error: "..." }` for 401, 403, 404, 409 and 500, including the JWT challenge and unknown routes.

## Alternatives considered

- **Switch to ProblemDetails and update the client at the same time.** It's the ASP.NET Core standard
  and I'd like to move to it, but changing both sides at once would mean the E2E tests no longer
  prove the new API is a drop-in replacement.

## Consequences

- The migration can be verified with the existing client and E2E tests.
- The API doesn't follow ASP.NET's standard error format yet. Logged in
  [tech-debt.md](../tech-debt.md) to revisit once the Angular client replaces the React one.
