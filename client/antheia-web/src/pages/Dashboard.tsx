import React from 'react';
import { Container, Typography, Paper } from '@mui/material';
import { useAuth } from '../features/auth/hooks/useAuth';

export const Dashboard: React.FC = () => {
  const { user } = useAuth();

  return (
    <Container maxWidth="lg">
      <Paper elevation={1} sx={{ p: 4, borderRadius: 2 }}>
        <Typography variant="h4" gutterBottom>
          Dashboard Overview
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Welcome back, {user?.username}. You have access under the role(s):{' '}
          <strong>{user?.roles?.join(', ')}</strong>.
        </Typography>
      </Paper>
    </Container>
  );
};