const express = require('express');
const db = require('../db');

const router = express.Router();

function validateRecipeBody(body) {
  const errors = [];
  if (!body.title || typeof body.title !== 'string' || !body.title.trim()) {
    errors.push('title is required');
  }
  if (
    !body.instructions ||
    typeof body.instructions !== 'string' ||
    !body.instructions.trim()
  ) {
    errors.push('instructions is required');
  }
  if (body.ingredients && !Array.isArray(body.ingredients)) {
    errors.push('ingredients must be an array');
  }
  if (body.tags && !Array.isArray(body.tags)) {
    errors.push('tags must be an array');
  }
  return errors;
}

// GET /api/recipes?search=&tag=
router.get('/', async (req, res, next) => {
  try {
    const { search, tag } = req.query;
    const params = [];
    let where = '1=1';

    if (search) {
      params.push(`%${search}%`);
      where += ` AND r.title ILIKE $${params.length}`;
    }
    if (tag) {
      params.push(tag);
      where += ` AND EXISTS (
        SELECT 1 FROM recipe_tags rt JOIN tags t ON t.id = rt.tag_id
        WHERE rt.recipe_id = r.id AND t.name = $${params.length}
      )`;
    }

    const { rows } = await db.query(
      `SELECT r.id, r.title, r.description, r.prep_minutes, r.cook_minutes,
              r.servings, r.image_url, r.created_at,
              COALESCE(array_agg(DISTINCT t.name) FILTER (WHERE t.name IS NOT NULL), '{}') AS tags
       FROM recipes r
       LEFT JOIN recipe_tags rt ON rt.recipe_id = r.id
       LEFT JOIN tags t ON t.id = rt.tag_id
       WHERE ${where}
       GROUP BY r.id
       ORDER BY r.created_at DESC`,
      params
    );
    res.json(rows);
  } catch (err) {
    next(err);
  }
});

// GET /api/recipes/:id
router.get('/:id', async (req, res, next) => {
  try {
    const { id } = req.params;
    const recipeResult = await db.query('SELECT * FROM recipes WHERE id = $1', [
      id,
    ]);
    if (recipeResult.rows.length === 0) {
      return res.status(404).json({ error: 'Recipe not found' });
    }
    const ingredientsResult = await db.query(
      'SELECT id, name, amount, unit FROM ingredients WHERE recipe_id = $1 ORDER BY position',
      [id]
    );
    const tagsResult = await db.query(
      `SELECT t.name FROM tags t
       JOIN recipe_tags rt ON rt.tag_id = t.id
       WHERE rt.recipe_id = $1`,
      [id]
    );
    res.json({
      ...recipeResult.rows[0],
      ingredients: ingredientsResult.rows,
      tags: tagsResult.rows.map((t) => t.name),
    });
  } catch (err) {
    next(err);
  }
});

// POST /api/recipes
router.post('/', async (req, res, next) => {
  const client = await db.pool.connect();
  try {
    const errors = validateRecipeBody(req.body);
    if (errors.length) return res.status(400).json({ errors });

    const {
      title,
      description = null,
      instructions,
      prep_minutes = 0,
      cook_minutes = 0,
      servings = 1,
      image_url = null,
      ingredients = [],
      tags = [],
    } = req.body;

    await client.query('BEGIN');

    const recipeResult = await client.query(
      `INSERT INTO recipes (title, description, instructions, prep_minutes, cook_minutes, servings, image_url)
       VALUES ($1,$2,$3,$4,$5,$6,$7) RETURNING *`,
      [title, description, instructions, prep_minutes, cook_minutes, servings, image_url]
    );
    const recipe = recipeResult.rows[0];

    for (let i = 0; i < ingredients.length; i++) {
      const ing = ingredients[i];
      await client.query(
        `INSERT INTO ingredients (recipe_id, name, amount, unit, position) VALUES ($1,$2,$3,$4,$5)`,
        [recipe.id, ing.name, ing.amount ?? null, ing.unit ?? null, i]
      );
    }

    for (const tagName of tags) {
      const tagResult = await client.query(
        `INSERT INTO tags (name) VALUES ($1)
         ON CONFLICT (name) DO UPDATE SET name = EXCLUDED.name RETURNING id`,
        [tagName]
      );
      await client.query(
        `INSERT INTO recipe_tags (recipe_id, tag_id) VALUES ($1,$2) ON CONFLICT DO NOTHING`,
        [recipe.id, tagResult.rows[0].id]
      );
    }

    await client.query('COMMIT');
    res.status(201).json({ id: recipe.id });
  } catch (err) {
    await client.query('ROLLBACK');
    next(err);
  } finally {
    client.release();
  }
});

// PUT /api/recipes/:id
router.put('/:id', async (req, res, next) => {
  try {
    const { id } = req.params;
    const errors = validateRecipeBody(req.body);
    if (errors.length) return res.status(400).json({ errors });

    const existing = await db.query('SELECT id FROM recipes WHERE id = $1', [id]);
    if (existing.rows.length === 0) {
      return res.status(404).json({ error: 'Recipe not found' });
    }

    const {
      title,
      description = null,
      instructions,
      prep_minutes = 0,
      cook_minutes = 0,
      servings = 1,
      image_url = null,
    } = req.body;

    await db.query(
      `UPDATE recipes SET title=$1, description=$2, instructions=$3, prep_minutes=$4,
       cook_minutes=$5, servings=$6, image_url=$7, updated_at=now() WHERE id=$8`,
      [title, description, instructions, prep_minutes, cook_minutes, servings, image_url, id]
    );

    res.json({ id: Number(id) });
  } catch (err) {
    next(err);
  }
});

// DELETE /api/recipes/:id
router.delete('/:id', async (req, res, next) => {
  try {
    const { id } = req.params;
    const result = await db.query('DELETE FROM recipes WHERE id = $1 RETURNING id', [
      id,
    ]);
    if (result.rows.length === 0) {
      return res.status(404).json({ error: 'Recipe not found' });
    }
    res.status(204).send();
  } catch (err) {
    next(err);
  }
});

module.exports = router;
