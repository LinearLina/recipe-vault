import React from 'react';
import { Routes, Route, Link } from 'react-router-dom';
import RecipeList from './pages/RecipeList.jsx';
import RecipeDetail from './pages/RecipeDetail.jsx';
import RecipeForm from './pages/RecipeForm.jsx';

export default function App() {
  return (
    <div className="app">
      <header className="topbar">
        <Link to="/" className="brand">
          <span className="brand-mark" aria-hidden="true">&#9733;</span>
          Recipe Vault
        </Link>
        <Link to="/recipes/new" className="btn btn-accent">+ New recipe</Link>
      </header>
      <main className="content">
        <Routes>
          <Route path="/" element={<RecipeList />} />
          <Route path="/recipes/new" element={<RecipeForm mode="create" />} />
          <Route path="/recipes/:id" element={<RecipeDetail />} />
          <Route path="/recipes/:id/edit" element={<RecipeForm mode="edit" />} />
        </Routes>
      </main>
    </div>
  );
}
