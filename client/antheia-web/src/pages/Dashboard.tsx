import React from 'react';
import { useAuth } from '../hooks/useAuth';

export const Dashboard: React.FC = () => {
  const { user, logout } = useAuth();

  return (
    <div>
      <h3>Welcome, {user?.username}!</h3>
      <p>Roles: {user?.roles.join(', ')}</p>
      <button onClick={logout}>Log Out</button>
    </div>
  );
};