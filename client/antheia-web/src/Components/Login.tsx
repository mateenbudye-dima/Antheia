import React, { useEffect, useState } from 'react';
import { isAxiosError } from 'axios';
import { login } from '../services/authService';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export const LoginForm: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const location = useLocation();

  const { isAuthenticated } = useAuth();

  useEffect(() => {
  if (isAuthenticated) {
    navigate('/dashboard', { replace: true });
  }
  }, [isAuthenticated, navigate]);

  // Retrieve origin route from state, or default to /dashboard
  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/dashboard';
  
  const handleSubmit = async (e: React.SubmitEvent) => {
    e.preventDefault();
    setError('');

    try {
      await login({ username, password });
      navigate(from, { replace: true }); // Navigate back to requested page
    } catch (err: unknown) {
      if (isAxiosError(err)) {
        // Safe access to Axios error response payload
        setError(err.response?.data?.message || 'Invalid login credentials');
      } else if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('An unexpected error occurred');
      }
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input 
        type="text" 
        value={username} 
        onChange={(e) => setUsername(e.target.value)} 
        placeholder="Username" 
      />
      <input 
        type="password" 
        value={password} 
        onChange={(e) => setPassword(e.target.value)} 
        placeholder="Password" 
      />
      <button type="submit">Login</button>
      {error && <p style={{ color: 'red' }}>{error}</p>}
    </form>
  );
};