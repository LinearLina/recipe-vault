import React from 'react';

export default function IngredientEditor({ ingredients, onChange }) {
  const update = (i, field, value) => {
    const next = ingredients.slice();
    next[i] = { ...next[i], [field]: value };
    onChange(next);
  };

  const remove = (i) => {
    onChange(ingredients.filter((_, idx) => idx !== i));
  };

  const add = () => {
    onChange([...ingredients, { name: '', amount: '', unit: '' }]);
  };

  return (
    <div className="ingredient-editor">
      {ingredients.map((ing, i) => (
        <div className="ingredient-row" key={i}>
          <input
            className="input input-amount"
            placeholder="200"
            value={ing.amount ?? ''}
            onChange={(e) => update(i, 'amount', e.target.value)}
          />
          <input
            className="input input-unit"
            placeholder="g"
            value={ing.unit ?? ''}
            onChange={(e) => update(i, 'unit', e.target.value)}
          />
          <input
            className="input input-name"
            placeholder="flour"
            value={ing.name ?? ''}
            onChange={(e) => update(i, 'name', e.target.value)}
          />
          <button
            type="button"
            className="btn btn-ghost btn-small"
            onClick={() => remove(i)}
            aria-label="Remove ingredient"
          >
            &times;
          </button>
        </div>
      ))}
      <button type="button" className="btn btn-ghost" onClick={add}>
        + Add ingredient
      </button>
    </div>
  );
}
