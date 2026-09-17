import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthProvider';

import { ProtectedRoute } from './Components/ProtectedRoute';
import { LoginForm } from './Components/Login';
import { Dashboard } from './pages/Dashboard';

export const App: React.FC = () => {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Root Redirect */}
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          {/* Public Routes */}
          <Route path="/login" element={<LoginForm />} />
          {/* <Route path="/unauthorized" element={<Unauthorized />} /> */}

          {/* Protected Routes (Requires Login) */}
          <Route element={<ProtectedRoute />}>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/profile" element={<div>User Profile</div>} />
          </Route>

          {/* Role-Gated Protected Routes (Requires 'Administrator' Role) 
          <Route element={<ProtectedRoute allowedRoles={['Administrator']} />}>
            <Route path="/admin" element={<AdminPanel />} />
          </Route>*/}
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
};