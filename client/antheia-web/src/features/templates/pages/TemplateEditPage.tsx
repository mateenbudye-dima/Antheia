import React, { useEffect, useMemo, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Typography,
  Paper,
  CircularProgress,
  Alert,
  Container,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';

import { EditorHeader } from '../components/EditorHeader';
import { TemplateEditor } from '../components/TemplateEditor';
import { SectionTree, type SectionNode } from '../components/SectionTree';
import { useTemplate } from '../hooks/useTemplate';
import { TemplateEditorProvider } from '../context/TemplateEditorContext';
import { useLayout } from '../../../shared/layouts/LayoutContext';
import { useTemplateEditorContext } from '../hooks/useTemplateEditorContext';

const TemplateEditContent: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const {
    setCustomSidebar,
    isSidebarCollapsed,
    toggleSidebar,
    toggleMobileSidebar,
  } = useLayout();

  // TanStack Query for server data
  const { data, isLoading, isError, error } = useTemplate(id);

  // Reducer Context for UI state
  const { state, setSelectedSection, setViewMode } = useTemplateEditorContext();
  const { selectedSectionId, viewMode } = state;

  const templateId = useMemo(() => (id ? parseInt(id, 10) : null), [id]);

  // Helper function to resolve human-readable labels for section types
  const getSectionTypeLabel = (typeId: number): string => {
    switch (typeId) {
      case 1:
        return 'Ingredients';
      case 2:
        return 'Preparation Method';
      case 3:
        return 'Evaluation Parameters';
      default:
        return 'Section';
    }
  };

  // ✅ FIXED: Dynamically map using sec.sectionId (Database Primary Key) instead of hardcoded type IDs
  const treeSections: SectionNode[] = useMemo(() => {
    const nodes: SectionNode[] = [{ id: 'header', title: 'Header & Overview' }];

    if (data?.sections && data.sections.length > 0) {
      // 1. Calculate counts for each section type
      const typeCounts: Record<number, number> = {};
      data.sections.forEach((sec) => {
        typeCounts[sec.sectionTypeId] = (typeCounts[sec.sectionTypeId] || 0) + 1;
      });

      const currentTypeIndex: Record<number, number> = {};

      // 2. Build unique node for EVERY section item
      data.sections.forEach((sec) => {
        const baseTitle = sec.sectionTitle || getSectionTypeLabel(sec.sectionTypeId);
        const totalOfThisType = typeCounts[sec.sectionTypeId] || 0;

        let title = baseTitle;
        // Append sequential numbers if there are duplicates (e.g., "Ingredients (1)", "Ingredients (2)")
        if (totalOfThisType > 1) {
          currentTypeIndex[sec.sectionTypeId] = (currentTypeIndex[sec.sectionTypeId] || 0) + 1;
          title = `${baseTitle} (${currentTypeIndex[sec.sectionTypeId]})`;
        }

        nodes.push({
          id: String(sec.sectionId), // 👈 Unique database section ID
          title,
        });
      });
    }

    return nodes;
  }, [data]);

  // Section Selection Callback
  const handleSelectSection = useCallback(
    (secId: string) => {
      setSelectedSection(secId);
      if (isMobile) {
        toggleMobileSidebar();
      }
    },
    [isMobile, toggleMobileSidebar, setSelectedSection]
  );

  // Inject Section Tree into Layout Sidebar
  useEffect(() => {
    if (viewMode === 'split') {
      setCustomSidebar(
        <SectionTree
          sections={treeSections}
          selectedSectionId={selectedSectionId}
          onSelectSection={handleSelectSection}
        />
      );
    } else {
      setCustomSidebar(null);
    }

    return () => {
      setCustomSidebar(null);
    };
  }, [
    viewMode,
    treeSections,
    selectedSectionId,
    setCustomSidebar,
    handleSelectSection,
  ]);

  const handleToggleClick = () => {
    if (isMobile) {
      toggleMobileSidebar();
    } else {
      toggleSidebar();
    }
  };

  if (!templateId || isNaN(templateId)) {
    return (
      <Container maxWidth="md" sx={{ py: 3 }}>
        <Paper variant="outlined" sx={{ p: 3 }}>
          <Typography variant="h6" color="error">
            Invalid Template ID
          </Typography>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate('/templates')}
            sx={{ mt: 2 }}
          >
            Back to Templates
          </Button>
        </Paper>
      </Container>
    );
  }

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 6 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (isError || !data) {
    return (
      <Container maxWidth="md" sx={{ py: 3 }}>
        <Alert severity="error">
          {error?.message || 'Template not found'}
        </Alert>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/templates')}
          sx={{ mt: 2 }}
        >
          Back to Templates
        </Button>
      </Container>
    );
  }

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        height: 'calc(100vh - 80px)',
        width: '100%',
        overflow: 'hidden',
      }}
    >
      {/* Sticky Header Box */}
      <Box
        sx={{
          position: 'sticky',
          top: 0,
          zIndex: (theme) => theme.zIndex.appBar - 1,
          backgroundColor: 'background.paper',
          py: 1,
          px: { xs: 1, sm: 2 },
          borderBottom: 1,
          borderColor: 'divider',
        }}
      >
        <EditorHeader
          title={data.title}
          templateId={templateId}
          viewMode={viewMode}
          isMobile={isMobile}
          isSidebarCollapsed={isSidebarCollapsed}
          onToggleSidebar={handleToggleClick}
          onBack={() => navigate('/templates')}
          onViewModeChange={(_e, newMode) => newMode && setViewMode(newMode)}
        />
      </Box>

      {/* Independently Scrollable Editor Box */}
      <Box
        sx={{
          flexGrow: 1,
          overflowY: 'auto',
          py: { xs: 1, sm: 1 },
        }}
      >
        <TemplateEditor
          data={data}
          selectedSectionId={selectedSectionId}
          viewMode={viewMode}
          onSectionDeleted={() => setSelectedSection('header')}
        />
      </Box>
    </Box>
  );
};

// Wrapper ensuring the provider is scoped specifically to this page
export const TemplateEditPage: React.FC = () => (
  <TemplateEditorProvider>
    <TemplateEditContent />
  </TemplateEditorProvider>
);