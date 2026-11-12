# 0006: JWT authentication and recipe ownership

- Status: Accepted
- Date: 2026-11-12

## Context

The Express API had started on auth (signup, login, a JWT middleware) but it wasn't finished:
there was no `users` table in the schema, and any logged-in user could edit or delete any recipe.

## Decision

- **Tokens:** ASP.NET Core JWT bearer authentication, HMAC-SHA256, 7-day expiry, user id in `sub`.
  Same flow as before, so the React client's `localStorage` token handling is unchanged.
- **Passwords:** ASP.NET Core Identity's `PasswordHasher` (PBKDF2 with a per-password salt), used on
  its own without the rest of Identity.
- **Ownership:** recipes store the creating user's id. Only the owner can update or delete (403
  otherwise). Reading stays public.
- **Sample recipes** from the seeder have no owner and are read-only.
- **Secrets:** the JWT secret is required at startup (at least 32 bytes) and comes from
  user-secrets or an environment variable. Only a clearly-labelled development value is committed.

## Alternatives considered

- **Full ASP.NET Core Identity** with its own tables, cookie auth, lockout and email confirmation.
  The right choice for a production app, but a lot of surface area for this project's needs.
- **Keep bcrypt** (BCrypt.Net) so existing password hashes keep working. There are no real users
  in the old database, so compatibility wasn't worth an extra dependency.

## Consequences

- Users from the old Postgres database can't log in with their old hashes. They need to sign up again.
- No refresh tokens, logout-everywhere or account lockout yet (in [tech-debt.md](../tech-debt.md)).
- Storing the token in `localStorage` is exposed to XSS; an httpOnly cookie would be safer.
