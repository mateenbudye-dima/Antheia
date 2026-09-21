import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from './features/auth/context/AuthProvider';
import { AppRoutes } from './routes/AppRoutes';
import { ColorModeProvider } from './context/ColorModeProvider';

export const App: React.FC = () => {
  return (
    <ColorModeProvider>
      <AuthProvider>
        <BrowserRouter>
          <AppRoutes />
        </BrowserRouter>
      </AuthProvider>
    </ColorModeProvider>
  );
};

export default App;