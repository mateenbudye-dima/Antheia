import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { AuthProvider } from './features/auth/context/AuthProvider.tsx'
import { App } from './App.tsx'
import { ThemeContextProvider } from './theme/ThemeContextProvider.tsx';
import { CssBaseline } from '@mui/material';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

// 1. Create a QueryClient instance outside the component
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider>
      <ThemeContextProvider>
        <CssBaseline />
        <QueryClientProvider client={queryClient}>
        <App />
        </QueryClientProvider>
      </ThemeContextProvider>
    </AuthProvider>
  </StrictMode>,
)
