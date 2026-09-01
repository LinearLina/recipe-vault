const express = require('express');
const cors = require('cors');
const recipesRouter = require('./routes/recipes');
const tagsRouter = require('./routes/tags');
const authRouter = require('./routes/auth');

function createApp() {
  const app = express();
  app.use(cors());
  app.use(express.json());

  app.get('/api/health', (req, res) => res.json({ status: 'ok' }));
  app.use('/api/recipes', recipesRouter);
  app.use('/api/tags', tagsRouter);
  app.use('/api', authRouter);

  // 404 handler
  app.use((req, res) => res.status(404).json({ error: 'Not found' }));

  // Central error handler
  app.use((err, req, res, next) => {
    console.error(err);
    res.status(500).json({ error: 'Internal server error' });
  });

  return app;
}

module.exports = createApp;
