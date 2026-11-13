# Tech debt log

Shortcuts and known gaps, written down on purpose so they get fixed rather than forgotten.
Update this when you take a shortcut, and close items in the PR that fixes them.

## Open

| # | Item | Why it exists | Impact | Plan |
|---|------|---------------|--------|------|
| 1 | Error responses use the old Express shape, not ProblemDetails | Kept the contract so the React client works unchanged ([ADR 0004](adr/0004-keep-the-existing-api-contract.md)) | Non-standard for .NET tooling and clients | Switch when the Angular client replaces React |
| 2 | React client still in place; Angular port not done | Backend migration first, one side at a time | Stack only partly matches .NET/Angular roles | Port the client to Angular + TypeScript |
| 3 | No pagination on `GET /api/recipes` | Parity with the old API (returns an array) | Slows down with many recipes | Add `page`/`page_size` with a response envelope, update the client |
| 4 | JWT stored in `localStorage`, no refresh tokens or lockout | Kept the existing client flow | XSS could steal a token; brute-force not limited | httpOnly cookie + refresh token, rate limiting on `/api/login` |
| 5 | Signup race: two requests for the same email at once | Duplicate check runs before insert | Second request gets a 500 instead of a 409 (unique index still protects data) | Catch the unique-constraint `DbUpdateException` and return 409 |
| 6 | Units are free text (`g`, `grams`, `cups`) | Kept from the original schema | Can't convert units or total a shopping list | Introduce a unit value object with a known list |
| 7 | Sample recipes have no owner and are read-only | Seeder runs before any user exists | Nobody can edit the sample recipes | Fine for a demo; seed an admin user if needed |
| 8 | No rate limiting or request size limits beyond defaults | Not needed for a portfolio app yet | Abuse possible if deployed publicly | ASP.NET Core rate-limiting middleware |
| 9 | Edit/Delete buttons show for recipes you don't own | Client doesn't know the current user's id | Clicking shows an error instead of the button being hidden (API correctly returns 403) | Return the user id at login and hide the buttons for non-owners |

## Fixed during the .NET migration

Found while reading the Express code before porting it. Each has a regression test.

| Bug in the Express API | Fix | Test |
|---|---|---|
| `POST /api/recipes` listed 8 columns but passed 7 values, so every create failed | EF Core builds the insert from the model | `A_created_recipe_comes_back_with_its_ingredients_in_order_and_its_tags` |
| Signup and recipe ownership used a `users` table and `user_id` column the migration never created | Both are in the `InitialCreate` migration; CI fails if migrations drift from the model | `Signup_returns_the_new_user_with_a_normalised_email` |
| API and E2E tests didn't log in after auth was added, so write tests got 401s and the suite failed | Tests sign up and log in through helpers | `LogInAsNewUserAsync` (xUnit), `logInAsNewUser` (Playwright) |
| `PUT` only updated the recipe row; ingredient and tag edits were silently lost | Update replaces ingredients and applies tag differences in one transaction | `Updating_a_recipe_replaces_its_ingredients_and_tags`, E2E `editing a recipe keeps ingredient and tag changes` |
| Any logged-in user could edit or delete anyone's recipe | Ownership check returns 403 | `Users_cannot_change_or_delete_someone_elses_recipe` |
| Missing password on signup threw inside `bcrypt.hash` outside the `try` block, an unhandled rejection that can crash the Node process | Validation returns 400 with a message | `Signup_rejects_short_passwords_and_missing_email` |
| Non-numeric ids (`/api/recipes/abc`) caused a database error (500) | Route constraint `{id:int}` returns 404 | Route constraint |
