const request = require('supertest');
const createApp = require('../src/app');
const db = require('../src/db');
const fs = require('fs');
const path = require('path');

const app = createApp();

beforeAll(async () => {
  // Fresh schema for the test run
  const sql = fs.readFileSync(
    path.join(__dirname, '../src/migrations/001_init.sql'),
    'utf8'
  );
  await db.query('DROP TABLE IF EXISTS recipe_tags, ingredients, tags, recipes CASCADE');
  await db.query(sql);
});

afterAll(async () => {
  await db.pool.end();
});

describe('GET /api/health', () => {
  it('returns ok', async () => {
    const res = await request(app).get('/api/health');
    expect(res.status).toBe(200);
    expect(res.body.status).toBe('ok');
  });
});

describe('Recipes API', () => {
  let createdId;

  it('rejects a recipe with no title (validation)', async () => {
    const res = await request(app)
      .post('/api/recipes')
      .send({ instructions: 'do stuff' });
    expect(res.status).toBe(400);
    expect(res.body.errors).toContain('title is required');
  });

  it('creates a recipe with ingredients and tags', async () => {
    const res = await request(app)
      .post('/api/recipes')
      .send({
        title: 'Test Pancakes',
        description: 'fluffy',
        instructions: 'mix, cook, flip',
        prep_minutes: 5,
        cook_minutes: 10,
        servings: 2,
        ingredients: [
          { name: 'flour', amount: 200, unit: 'g' },
          { name: 'egg', amount: 2, unit: null },
        ],
        tags: ['breakfast', 'quick'],
      });
    expect(res.status).toBe(201);
    expect(res.body.id).toBeDefined();
    createdId = res.body.id;
  });

  it('fetches the created recipe with its relations', async () => {
    const res = await request(app).get(`/api/recipes/${createdId}`);
    expect(res.status).toBe(200);
    expect(res.body.title).toBe('Test Pancakes');
    expect(res.body.ingredients).toHaveLength(2);
    expect(res.body.tags.sort()).toEqual(['breakfast', 'quick']);
  });

  it('lists recipes and finds it by tag filter', async () => {
    const res = await request(app).get('/api/recipes?tag=breakfast');
    expect(res.status).toBe(200);
    expect(res.body.some((r) => r.id === createdId)).toBe(true);
  });

  it('lists recipes and finds it by search', async () => {
    const res = await request(app).get('/api/recipes?search=Pancake');
    expect(res.status).toBe(200);
    expect(res.body.some((r) => r.id === createdId)).toBe(true);
  });

  it('updates the recipe', async () => {
    const res = await request(app)
      .put(`/api/recipes/${createdId}`)
      .send({
        title: 'Test Pancakes v2',
        instructions: 'mix, cook, flip, serve',
      });
    expect(res.status).toBe(200);

    const getRes = await request(app).get(`/api/recipes/${createdId}`);
    expect(getRes.body.title).toBe('Test Pancakes v2');
  });

  it('404s for an unknown recipe id', async () => {
    const res = await request(app).get('/api/recipes/999999');
    expect(res.status).toBe(404);
  });

  it('deletes the recipe', async () => {
    const res = await request(app).delete(`/api/recipes/${createdId}`);
    expect(res.status).toBe(204);

    const getRes = await request(app).get(`/api/recipes/${createdId}`);
    expect(getRes.status).toBe(404);
  });
});
