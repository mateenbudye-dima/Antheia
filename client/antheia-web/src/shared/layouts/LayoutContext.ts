import { createContext, useContext } from 'react';
import type { LayoutContextType } from '../types/types';

export const LayoutContext = createContext<LayoutContextType | undefined>(
  undefined
);

export const useLayout = (): LayoutContextType => {
  const context = useContext(LayoutContext);
  if (!context) {
    throw new Error('useLayout must be used within a LayoutProvider');
  }
  return context;
};