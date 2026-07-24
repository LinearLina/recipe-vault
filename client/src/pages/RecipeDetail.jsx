import React, { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { api } from '../api.js';

export default function RecipeDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [recipe, setRecipe] = useState(null);
  const [status, setStatus] = useState('loading');

  useEffect(() => {
    setStatus('loading');
    api
      .getRecipe(id)
      .then((data) => {
        setRecipe(data);
        setStatus('ready');
      })
      .catch(() => setStatus('error'));
  }, [id]);

  const handleDelete = async () => {
    if (!window.confirm(`Delete "${recipe.title}"? This can't be undone.`)) return;
    await api.deleteRecipe(id);
    navigate('/');
  };

  if (status === 'loading') return <p className="empty-state">Loading&hellip;</p>;
  if (status === 'error' || !recipe)
    return (
      <div>
        <p className="empty-state">That recipe doesn't exist, or was deleted.</p>
        <Link to="/" className="btn btn-outline">Back to the recipe box</Link>
      </div>
    );

  const totalTime = (recipe.prep_minutes || 0) + (recipe.cook_minutes || 0);

  return (
    <article className="detail-card">
      <Link to="/" className="back-link">&larr; Back to the recipe box</Link>
      <h1 className="detail-title">{recipe.title}</h1>
      {recipe.description && <p className="detail-desc">{recipe.description}</p>}

      {recipe.tags?.length > 0 && (
        <div className="tab-row">
          {recipe.tags.map((t) => (
            <span key={t} className="tab">{t}</span>
          ))}
        </div>
      )}

      <div className="detail-meta">
        <div><span className="meta-label">Prep</span>{recipe.prep_minutes} min</div>
        <div><span className="meta-label">Cook</span>{recipe.cook_minutes} min</div>
        <div><span className="meta-label">Total</span>{totalTime} min</div>
        <div><span className="meta-label">Servings</span>{recipe.servings}</div>
      </div>

      <div className="detail-body">
        <section>
          <h2>Ingredients</h2>
          <ul className="ingredient-list">
            {recipe.ingredients.map((ing) => (
              <li key={ing.id}>
                <span className="ing-amount">
                  {ing.amount ?? ''} {ing.unit ?? ''}
                </span>
                <span className="ing-name">{ing.name}</span>
              </li>
            ))}
          </ul>
        </section>
        <section>
          <h2>Instructions</h2>
          <p className="instructions">{recipe.instructions}</p>
        </section>
      </div>

      <div className="detail-actions">
        <Link to={`/recipes/${id}/edit`} className="btn btn-outline">Edit</Link>
        <button className="btn btn-danger" onClick={handleDelete}>Delete</button>
      </div>
    </article>
  );
}
