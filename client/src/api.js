const BASE = '/api';

const TOKEN_KEY = 'recipe_vault_token';

export function saveToken(token) {
  localStorage.setItem(TOKEN_KEY, token);
}

export function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
}

function authHeaders() {
  const token = getToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
}

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
  signup: (email, password) =>
    fetch(`${BASE}/signup`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    }).then(handle),

  login: (email, password) =>
    fetch(`${BASE}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    }).then(handle),

  listRecipes: (params = {}) => {
    const qs = new URLSearchParams(params).toString();
    return fetch(`${BASE}/recipes${qs ? `?${qs}` : ''}`).then(handle);
  },
  getRecipe: (id) => fetch(`${BASE}/recipes/${id}`).then(handle),
  createRecipe: (body) =>
    fetch(`${BASE}/recipes`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...authHeaders() },
      body: JSON.stringify(body),
    }).then(handle),
  updateRecipe: (id, body) =>
    fetch(`${BASE}/recipes/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json', ...authHeaders() },
      body: JSON.stringify(body),
    }).then(handle),
  deleteRecipe: (id) =>
    fetch(`${BASE}/recipes/${id}`, {
      method: 'DELETE',
      headers: { ...authHeaders() },
    }).then(handle),
  listTags: () => fetch(`${BASE}/tags`).then(handle),
};