const db = require('./db');

async function seed() {
  await db.query('DELETE FROM recipe_tags');
  await db.query('DELETE FROM ingredients');
  await db.query('DELETE FROM tags');
  await db.query('DELETE FROM recipes');

  const tagNames = ['vegetarian', 'quick', 'dessert', 'italian', 'spicy'];
  const tagIds = {};
  for (const name of tagNames) {
    const { rows } = await db.query(
      'INSERT INTO tags (name) VALUES ($1) RETURNING id',
      [name]
    );
    tagIds[name] = rows[0].id;
  }

  const recipe1 = await db.query(
    `INSERT INTO recipes (title, description, instructions, prep_minutes, cook_minutes, servings)
     VALUES ($1,$2,$3,$4,$5,$6) RETURNING id`,
    [
      'Fifteen-Minute Garlic Noodles',
      'A weeknight staple: salty, garlicky, done before the kettle cools.',
      '1. Boil noodles.\n2. Fry garlic in butter and soy sauce.\n3. Toss together.',
      5,
      10,
      2,
    ]
  );
  const r1 = recipe1.rows[0].id;
  await db.query(
    `INSERT INTO ingredients (recipe_id, name, amount, unit, position) VALUES
     ($1,'egg noodles',200,'g',0), ($1,'garlic cloves',6,null,1), ($1,'soy sauce',3,'tbsp',2), ($1,'butter',2,'tbsp',3)`,
    [r1]
  );
  await db.query(
    `INSERT INTO recipe_tags (recipe_id, tag_id) VALUES ($1,$2), ($1,$3)`,
    [r1, tagIds['quick'], tagIds['vegetarian']]
  );

  const recipe2 = await db.query(
    `INSERT INTO recipes (title, description, instructions, prep_minutes, cook_minutes, servings)
     VALUES ($1,$2,$3,$4,$5,$6) RETURNING id`,
    [
      'Classic Tiramisu',
      'No-bake Italian dessert, layered and chilled.',
      '1. Whisk mascarpone mixture.\n2. Dip ladyfingers in coffee.\n3. Layer and chill overnight.',
      30,
      0,
      8,
    ]
  );
  const r2 = recipe2.rows[0].id;
  await db.query(
    `INSERT INTO ingredients (recipe_id, name, amount, unit, position) VALUES
     ($1,'mascarpone',500,'g',0), ($1,'ladyfingers',24,null,1), ($1,'espresso',300,'ml',2), ($1,'cocoa powder',2,'tbsp',3)`,
    [r2]
  );
  await db.query(
    `INSERT INTO recipe_tags (recipe_id, tag_id) VALUES ($1,$2), ($1,$3)`,
    [r2, tagIds['dessert'], tagIds['italian']]
  );

  console.log('Seed complete.');
  await db.pool.end();
}

seed().catch((err) => {
  console.error('Seed failed:', err);
  process.exit(1);
});
