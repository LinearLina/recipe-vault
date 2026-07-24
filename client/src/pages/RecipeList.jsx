import React, { useEffect, useState, useCallback } from 'react';
import { api } from '../api.js';
import RecipeCard from '../components/RecipeCard.jsx';

export default function RecipeList() {
  const [recipes, setRecipes] = useState([]);
  const [tags, setTags] = useState([]);
  const [search, setSearch] = useState('');
  const [activeTag, setActiveTag] = useState('');
  const [status, setStatus] = useState('loading'); // loading | ready | error

  const load = useCallback(async (params) => {
    setStatus('loading');
    try {
      const data = await api.listRecipes(params);
      setRecipes(data);
      setStatus('ready');
    } catch (err) {
      setStatus('error');
    }
  }, []);

  useEffect(() => {
    load({});
    api.listTags().then(setTags).catch(() => {});
  }, [load]);

  const applyFilters = (e) => {
    e?.preventDefault();
    const params = {};
    if (search.trim()) params.search = search.trim();
    if (activeTag) params.tag = activeTag;
    load(params);
  };

  const clickTag = (tagName) => {
    const next = activeTag === tagName ? '' : tagName;
    setActiveTag(next);
    const params = {};
    if (search.trim()) params.search = search.trim();
    if (next) params.tag = next;
    load(params);
  };

  return (
    <div>
      <div className="page-head">
        <h1>The recipe box</h1>
        <p className="page-sub">
          Every recipe you'd hate to lose, filed and searchable.
        </p>
      </div>

      <form className="filter-bar" onSubmit={applyFilters}>
        <input
          className="input"
          placeholder="Search by title..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <button className="btn btn-outline" type="submit">Search</button>
      </form>

      {tags.length > 0 && (
        <div className="tab-row tab-row-filter">
          {tags.map((t) => (
            <button
              key={t.id}
              type="button"
              className={`tab tab-clickable ${activeTag === t.name ? 'tab-active' : ''}`}
              onClick={() => clickTag(t.name)}
            >
              {t.name}
            </button>
          ))}
        </div>
      )}

      {status === 'loading' && <p className="empty-state">Loading recipes&hellip;</p>}
      {status === 'error' && (
        <p className="empty-state">
          Couldn't reach the API. Is the server running on port 4000?
        </p>
      )}
      {status === 'ready' && recipes.length === 0 && (
        <p className="empty-state">
          No recipes match yet. Try a different search, or add one.
        </p>
      )}

      <div className="card-grid">
        {recipes.map((r) => (
          <RecipeCard key={r.id} recipe={r} />
        ))}
      </div>
    </div>
  );
}
