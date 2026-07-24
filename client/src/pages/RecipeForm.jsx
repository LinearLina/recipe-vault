import React, { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { api } from '../api.js';
import IngredientEditor from '../components/IngredientEditor.jsx';

const EMPTY = {
  title: '',
  description: '',
  instructions: '',
  prep_minutes: 0,
  cook_minutes: 0,
  servings: 1,
  ingredients: [{ name: '', amount: '', unit: '' }],
  tags: '',
};

export default function RecipeForm({ mode }) {
  const { id } = useParams();
  const navigate = useNavigate();
  const [form, setForm] = useState(EMPTY);
  const [errors, setErrors] = useState([]);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (mode === 'edit' && id) {
      api.getRecipe(id).then((data) => {
        setForm({
          ...data,
          tags: (data.tags || []).join(', '),
          ingredients:
            data.ingredients.length > 0
              ? data.ingredients
              : [{ name: '', amount: '', unit: '' }],
        });
      });
    }
  }, [mode, id]);

  const field = (name) => ({
    value: form[name],
    onChange: (e) => setForm({ ...form, [name]: e.target.value }),
  });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrors([]);
    setSaving(true);
    const payload = {
      ...form,
      prep_minutes: Number(form.prep_minutes) || 0,
      cook_minutes: Number(form.cook_minutes) || 0,
      servings: Number(form.servings) || 1,
      ingredients: form.ingredients
        .filter((i) => i.name?.trim())
        .map((i) => ({ ...i, amount: i.amount === '' ? null : Number(i.amount) })),
      tags: form.tags
        .split(',')
        .map((t) => t.trim())
        .filter(Boolean),
    };

    try {
      if (mode === 'create') {
        const res = await api.createRecipe(payload);
        navigate(`/recipes/${res.id}`);
      } else {
        await api.updateRecipe(id, payload);
        navigate(`/recipes/${id}`);
      }
    } catch (err) {
      setErrors([err.message]);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="form-card">
      <Link to={mode === 'edit' ? `/recipes/${id}` : '/'} className="back-link">
        &larr; Cancel
      </Link>
      <h1>{mode === 'create' ? 'Add a recipe' : 'Edit recipe'}</h1>

      {errors.length > 0 && (
        <div className="form-errors">
          {errors.map((err) => <p key={err}>{err}</p>)}
        </div>
      )}

      <form onSubmit={handleSubmit} className="recipe-form">
        <label>
          Title
          <input className="input" required {...field('title')} />
        </label>

        <label>
          Short description
          <input className="input" {...field('description')} />
        </label>

        <div className="form-row">
          <label>
            Prep (min)
            <input className="input" type="number" min="0" {...field('prep_minutes')} />
          </label>
          <label>
            Cook (min)
            <input className="input" type="number" min="0" {...field('cook_minutes')} />
          </label>
          <label>
            Servings
            <input className="input" type="number" min="1" {...field('servings')} />
          </label>
        </div>

        <label>
          Tags (comma-separated)
          <input className="input" placeholder="quick, vegetarian" {...field('tags')} />
        </label>

        <div>
          <p className="field-label">Ingredients</p>
          <IngredientEditor
            ingredients={form.ingredients}
            onChange={(ingredients) => setForm({ ...form, ingredients })}
          />
        </div>

        <label>
          Instructions
          <textarea
            className="input textarea"
            required
            rows={8}
            {...field('instructions')}
          />
        </label>

        <button className="btn btn-accent" type="submit" disabled={saving}>
          {saving ? 'Saving…' : mode === 'create' ? 'Save recipe' : 'Save changes'}
        </button>
      </form>
    </div>
  );
}
