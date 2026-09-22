import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from './features/auth/context/AuthProvider';
import { AppRoutes } from './routes/AppRoutes';
import { ColorModeProvider } from './context/ColorModeProvider';
import { LayoutProvider } from './shared/layouts/LayoutProvider';

export const App: React.FC = () => {
  return (
    <ColorModeProvider>
      <AuthProvider>
        <LayoutProvider>
          <BrowserRouter>
            <AppRoutes />
          </BrowserRouter>
        </LayoutProvider>
      </AuthProvider>
    </ColorModeProvider>
  );
};

export default App;