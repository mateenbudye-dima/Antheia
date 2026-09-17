import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

interface ProtectedRouteProps {
  allowedRoles?: string[];
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ allowedRoles }) => {
  const { user, isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  // 1. Wait for AuthContext to finish reading localStorage/JWT token
  if (isLoading) {
    return <div>Loading...</div>; // Or a spinner
  }

  // 2. Redirect to /login if not authenticated, preserving current route for redirect back
  if (!isAuthenticated || !user) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // 3. Optional: Role-based guard check
  if (allowedRoles && !allowedRoles.some((role) => user.roles.includes(role))) {
    return <Navigate to="/unauthorized" replace />;
  }

  // 4. Render child route components via Outlet
  return <Outlet />;
};