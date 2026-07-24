# Recipe Vault

A full-stack recipe organizer built to demonstrate end-to-end web development:
React (Vite) frontend, Node/Express REST API, PostgreSQL database with proper
relational modeling, automated tests at three levels (unit, API/integration,
and E2E), and a CI pipeline.

## Why this project

Built as a portfolio piece for junior developer / QA roles. It deliberately
covers the things those roles ask about:

- **Relational DB design**: recipes → ingredients (one-to-many), recipes ↔
  tags (many-to-many via a join table), foreign keys with cascading deletes.
- **REST API**: full CRUD, filtering/search via query params, input validation,
  proper HTTP status codes.
- **Testing pyramid**: Jest unit tests, Supertest API/integration tests against
  a real Postgres instance, and Playwright E2E tests driving the actual UI.
- **CI/CD**: GitHub Actions runs the whole test suite (with a real Postgres
  service container) on every push.

## Stack

| Layer      | Tech                          |
|------------|--------------------------------|
| Frontend   | React 18, Vite, plain CSS      |
| Backend    | Node.js, Express               |
| Database   | PostgreSQL                     |
| Testing    | Jest, Supertest, Playwright    |
| CI         | GitHub Actions                 |
| Local DB   | Docker Compose                 |

## Project structure

```
recipe-vault/
├── docker-compose.yml       # spins up Postgres locally
├── server/                  # Express API
│   ├── src/
│   │   ├── index.js         # app entrypoint
│   │   ├── db.js            # pg pool
│   │   ├── routes/          # recipes, tags
│   │   └── migrations/      # SQL schema
│   └── tests/                # Supertest API tests
├── client/                  # React (Vite) frontend
│   └── src/
│       ├── pages/           # RecipeList, RecipeDetail, RecipeForm
│       ├── components/
│       └── api.js           # fetch wrapper
├── e2e/                      # Playwright end-to-end tests
└── .github/workflows/ci.yml
```

## Running it locally

### 1. Database
```bash
docker compose up -d          # starts Postgres on localhost:5432
cd server
npm install
npm run migrate               # creates tables
npm run seed                  # optional sample data
```

### 2. API server
```bash
cd server
cp .env.example .env          # adjust if needed
npm run dev                   # http://localhost:4000
```

### 3. Frontend
```bash
cd client
npm install
npm run dev                   # http://localhost:5173
```

## Running the tests

```bash
# API + unit tests (needs Postgres running, see docker-compose)
cd server && npm test

# End-to-end tests (needs both server and client running)
cd e2e && npm install && npx playwright install --with-deps && npm test
```

## API reference

| Method | Route              | Description                          |
|--------|---------------------|---------------------------------------|
| GET    | /api/recipes         | List recipes, `?search=` `&tag=`     |
| GET    | /api/recipes/:id      | Get one recipe with ingredients+tags |
| POST   | /api/recipes         | Create a recipe                      |
| PUT    | /api/recipes/:id      | Update a recipe                      |
| DELETE | /api/recipes/:id      | Delete a recipe                      |
| GET    | /api/tags             | List all tags                        |

## What I'd add next

- Auth (JWT) so recipes belong to a user
- Image upload to S3/Cloud storage instead of a URL field
- Pagination on the recipe list
- Optimistic UI updates on the frontend
