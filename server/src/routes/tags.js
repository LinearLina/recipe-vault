const express = require('express');
const db = require('../db');

const router = express.Router();

router.get('/', async (req, res, next) => {
  try {
    const { rows } = await db.query('SELECT id, name FROM tags ORDER BY name');
    res.json(rows);
  } catch (err) {
    next(err);
  }
});

module.exports = router;
