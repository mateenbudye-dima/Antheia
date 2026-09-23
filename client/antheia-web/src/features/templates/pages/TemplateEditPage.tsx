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

  // Build Tree Structure
  const treeSections: SectionNode[] = useMemo(() => {
    const nodes: SectionNode[] = [{ id: 'header', title: 'Header & Overview' }];

    if (data?.sections) {
      data.sections.forEach((sec) => {
        if (sec.sectionTypeId === 1) {
          nodes.push({ id: 'section_1', title: 'Ingredients' });
        } else if (sec.sectionTypeId === 2) {
          nodes.push({ id: 'section_2', title: 'Preparation Method' });
        } else if (sec.sectionTypeId === 3) {
          nodes.push({ id: 'section_3', title: 'Evaluation Parameters' });
        }
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
        height: 'calc(100vh - 80px)', // Adjust 80px according to your main top Navbar height
        width: '100%',
        overflow: 'hidden', // Prevents double scrollbars on main window
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