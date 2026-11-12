import React from 'react';
import { Link } from 'react-router-dom';

export default function RecipeCard({ recipe }) {
  const totalTime = (recipe.prep_minutes || 0) + (recipe.cook_minutes || 0);
  return (
    <Link to={`/recipes/${recipe.id}`} className="index-card">
      <div className="index-card-hole" aria-hidden="true" />
      <h3 className="index-card-title">{recipe.title}</h3>
      {recipe.description && (
        <p className="index-card-desc">{recipe.description}</p>
      )}
      <div className="index-card-meta">
        <span>{totalTime} min</span>
        <span>&middot;</span>
        <span>{recipe.servings} servings</span>
      </div>
      {recipe.tags?.length > 0 && (
        <div className="tab-row">
          {recipe.tags.map((t) => (
            <span key={t} className="tab">{t}</span>
          ))}
        </div>
      )}
    </Link>
  );
}
