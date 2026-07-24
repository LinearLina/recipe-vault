const BASE = '/api';

async function handle(res) {
  if (res.status === 204) return null;
  const data = await res.json().catch(() => null);
  if (!res.ok) {
    const message = data?.errors?.join(', ') || data?.error || 'Request failed';
    throw new Error(message);
  }
  return data;
}

export const api = {
  listRecipes: (params = {}) => {
    const qs = new URLSearchParams(params).toString();
    return fetch(`${BASE}/recipes${qs ? `?${qs}` : ''}`).then(handle);
  },
  getRecipe: (id) => fetch(`${BASE}/recipes/${id}`).then(handle),
  createRecipe: (body) =>
    fetch(`${BASE}/recipes`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    }).then(handle),
  updateRecipe: (id, body) =>
    fetch(`${BASE}/recipes/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    }).then(handle),
  deleteRecipe: (id) =>
    fetch(`${BASE}/recipes/${id}`, { method: 'DELETE' }).then(handle),
  listTags: () => fetch(`${BASE}/tags`).then(handle),
};
