// src/routes/AppRoutes.tsx
import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { LoginForm } from '../features/auth/Components/Login';
import { Dashboard } from '../pages/Dashboard';
import { Layout } from '../shared/layouts/Layout';
import { TemplateEditPage, TemplateListPage } from '../features/templates';
import { ProtectedRoute } from '../shared/auth/ProtectedRoute';

export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      {/* Root Redirect */}
      <Route path="/" element={<Navigate to="/dashboard" replace />} />

      {/* Public Routes */}
      <Route path="/login" element={<LoginForm />} />

      {/* Protected Routes Wrapped in Layout */}
      <Route element={<ProtectedRoute />}>
        <Route element={<Layout />}>
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/profile" element={<div>User Profile Page</div>} />
          {/* List of Templates */}
          <Route path="/templates" element={<TemplateListPage />} />

          {/* Edit Template Route with Dynamic ID */}
          <Route path="/templates/:id/edit" element={<TemplateEditPage />} />
        </Route>
      </Route>

      {/* Fallback Catch-all */}
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
};