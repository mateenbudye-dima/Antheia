import React, { useState, useCallback } from 'react';
import { LayoutContext } from './LayoutContext';
import type { LayoutProviderProps } from '../types/types';

export const LayoutProvider: React.FC<LayoutProviderProps> = ({ children }) => {
  const [customSidebar, setCustomSidebar] = useState<React.ReactNode | null>(null);
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState<boolean>(false);
  const [isMobileSidebarOpen, setIsMobileSidebarOpen] = useState<boolean>(false);

  const toggleSidebar = useCallback(() => {
    setIsSidebarCollapsed((prev) => !prev);
  }, []);

  const toggleMobileSidebar = useCallback(() => {
    setIsMobileSidebarOpen((prev) => !prev);
  }, []);

  return (
    <LayoutContext.Provider
      value={{
        customSidebar,
        setCustomSidebar,
        isSidebarCollapsed,
        setIsSidebarCollapsed,
        toggleSidebar,
        isMobileSidebarOpen,
        setIsMobileSidebarOpen,
        toggleMobileSidebar,
      }}
    >
      {children}
    </LayoutContext.Provider>
  );
};