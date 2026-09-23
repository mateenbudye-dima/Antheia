import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from './features/auth/context/AuthProvider';
import { AppRoutes } from './routes/AppRoutes';
import { ColorModeProvider } from './context/ColorModeProvider';
import { LayoutProvider } from './shared/layouts/LayoutProvider';
import { DialogProvider } from './shared/context/DialogProvider';

export const App: React.FC = () => {
  return (
    <ColorModeProvider>
      <AuthProvider>
        <DialogProvider>
          <LayoutProvider>
            <BrowserRouter>
              <AppRoutes />
            </BrowserRouter>
          </LayoutProvider>
        </DialogProvider>
      </AuthProvider>
    </ColorModeProvider>
  );
};

export default App;