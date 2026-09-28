// src/routes/AppRoutes.tsx
import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { LoginForm } from '../features/auth/Components/Login';
import { Dashboard } from '../pages/Dashboard';
import { Layout } from '../shared/layouts/Layout';
import { BlendEditPage, BlendListPage } from '../features/blends';
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
          {/* List of Blends */}
          <Route path="/blends" element={<BlendListPage />} />

          {/* Edit Blend Route with Dynamic ID */}
          <Route path="/blends/:id/edit" element={<BlendEditPage />} />
        </Route>
      </Route>

      {/* Fallback Catch-all */}
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
};