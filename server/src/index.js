const createApp = require('./app');

const app = createApp();
const PORT = process.env.PORT || 4000;

app.listen(PORT, () => {
  console.log(`Recipe Vault API listening on port ${PORT}`);
});
