# 0005: Return results for expected failures, throw only for bugs

- Status: Accepted
- Date: 2026-11-12

## Context

Services need to report "not found", "not your recipe", "invalid input" and "email already in use".
These are normal outcomes, not bugs.

## Decision

Application services return `Result<T>` with a status (`Ok`, `Invalid`, `NotFound`, `Forbidden`,
`Conflict`, `Unauthorized`). Controllers map statuses to HTTP codes in one place
(`ApiControllerBase.Failure`). Exceptions are reserved for things that shouldn't happen:

- `DomainValidationException`: a domain rule was broken that the request validator should have
  caught. Returned as 400 so a validator gap is still the caller's error, not a 500.
- Anything else: logged and returned as 500 by `GlobalExceptionHandler`.

## Alternatives considered

- **Throw `NotFoundException` etc. and map them in middleware.** Less code in controllers, but
  failure paths become invisible in method signatures and easy to forget to handle.
- **A library such as FluentResults or ErrorOr.** More features than needed for six statuses.

## Consequences

- A method's possible failures are visible in its return type and handled explicitly.
- Validation lives in two places on purpose: the request validator (friendly messages, all errors
  at once) and the domain entities (a safety net that guarantees invalid state can't be saved).
