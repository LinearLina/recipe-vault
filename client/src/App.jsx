import React from 'react';
import { Routes, Route, Link } from 'react-router-dom';
import RecipeList from './pages/RecipeList.jsx';
import RecipeDetail from './pages/RecipeDetail.jsx';
import RecipeForm from './pages/RecipeForm.jsx';
import Login from './pages/Login.jsx';
import Signup from './pages/Signup.jsx';
import { getToken, clearToken } from './api.js';

export default function App() {
  const isLoggedIn = !!getToken();

  function handleLogout() {
    clearToken();
    window.location.href = '/';
  }

  return (
    <div className="app">
      <header className="topbar">
        <Link to="/" className="brand">
          <span className="brand-mark" aria-hidden="true">&#9733;</span>
          Recipe Vault
        </Link>
        <Link to="/recipes/new" className="btn btn-accent">+ New recipe</Link>
        {isLoggedIn ? (
          <button onClick={handleLogout} className="btn">Log out</button>
        ) : (
          <Link to="/login" className="btn">Log in</Link>
        )}
      </header>
      <main className="content">
        <Routes>
          <Route path="/" element={<RecipeList />} />
          <Route path="/recipes/new" element={<RecipeForm mode="create" />} />
          <Route path="/recipes/:id" element={<RecipeDetail />} />
          <Route path="/recipes/:id/edit" element={<RecipeForm mode="edit" />} />
          <Route path="/login" element={<Login />} />
          <Route path="/signup" element={<Signup />} />
        </Routes>
      </main>
    </div>
  );
}