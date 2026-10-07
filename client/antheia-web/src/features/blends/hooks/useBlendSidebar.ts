import { useEffect } from 'react';
import { useLayout } from '../../../shared/layouts/LayoutContext';

export const useBlendSidebar = (
  viewMode: 'split' | 'all',
  sidebarContent: React.ReactNode
) => {
  const { setCustomSidebar } = useLayout();

  useEffect(() => {
    if (viewMode === 'split') {
      setCustomSidebar(sidebarContent);
    } else {
      setCustomSidebar(null);
    }

    return () => setCustomSidebar(null);
  }, [viewMode, sidebarContent, setCustomSidebar]);
};