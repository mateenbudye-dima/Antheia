import type { ReactNode, Dispatch, SetStateAction } from 'react';

export interface LayoutContextType {
  customSidebar: ReactNode | null;
  setCustomSidebar: (node: ReactNode | null) => void;
  isSidebarCollapsed: boolean;
  setIsSidebarCollapsed: Dispatch<SetStateAction<boolean>>;
  toggleSidebar: () => void;
  isMobileSidebarOpen: boolean;
  setIsMobileSidebarOpen: Dispatch<SetStateAction<boolean>>;
  toggleMobileSidebar: () => void;
}

export interface LayoutProviderProps {
  children: ReactNode;
}