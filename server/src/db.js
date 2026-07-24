require('dotenv').config();
const { Pool } = require('pg');

const pool = new Pool({
  connectionString:
    process.env.DATABASE_URL ||
    'postgresql://recipevault:recipevault@localhost:5432/recipevault',
});

module.exports = {
  query: (text, params) => pool.query(text, params),
  pool,
};
