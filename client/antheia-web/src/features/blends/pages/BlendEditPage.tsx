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
import { BlendEditor } from '../components/BlendEditor';
import { SectionTree, type SectionNode } from '../components/SectionTree';
import { useBlend } from '../hooks/useBlend';
import { BlendEditorProvider } from '../context/BlendEditorContext';
import { useLayout } from '../../../shared/layouts/LayoutContext';
import { useBlendEditorContext } from '../hooks/useBlendEditorContext';
import { SectionType, type Section } from '../types/blend.types';
import { NodeType } from '../types/editor.types';

// Helper function to resolve human-readable labels for section types using NodeType enum
const getSectionTypeLabel = (typeId: number): string => {
  switch (typeId) {
    case SectionType.Ingredients:
      return 'Ingredients';
    case SectionType.PreparationMethod:
      return 'Preparation Method';
    case SectionType.Evaluation:
      return 'Evaluation Parameters';
    default:
      return 'Section';
  }
};

interface InnerProps {
  blendId: number;
}

// 1. Inner Component - Only receives and handles a guaranteed numeric blendId
const BlendEditContentInner: React.FC<InnerProps> = ({ blendId }) => {
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const {
    setCustomSidebar,
    isSidebarCollapsed,
    toggleSidebar,
    toggleMobileSidebar,
  } = useLayout();

  // TanStack Query for server data (blendId is strictly number)
  const { data, isLoading, isError, error } = useBlend(blendId);

  // Reducer Context for UI state
  const { state, setSelectedSection, setViewMode } = useBlendEditorContext();
  const { selectedSectionId, viewMode } = state;

  const treeSections: SectionNode[] = useMemo(() => {
    const nodes: SectionNode[] = [
      { id: 'header', title: 'Header & Overview', nodeType: NodeType.Header },
    ];

    if (data?.sections && data.sections.length > 0) {
      // Calculate counts for each section type
      const typeCounts: Record<number, number> = {};
      data.sections.forEach((sec: Section) => {
        typeCounts[sec.sectionTypeId] = (typeCounts[sec.sectionTypeId] || 0) + 1;
      });

      const currentTypeIndex: Record<number, number> = {};

      // Build unique node for EVERY section item
      data.sections.forEach((sec: Section) => {
        const baseTitle =
          sec.sectionTitle || getSectionTypeLabel(sec.sectionTypeId);
        const totalOfThisType = typeCounts[sec.sectionTypeId] || 0;

        let title = baseTitle;
        if (totalOfThisType > 1) {
          currentTypeIndex[sec.sectionTypeId] =
            (currentTypeIndex[sec.sectionTypeId] || 0) + 1;
          title = `${baseTitle} (${currentTypeIndex[sec.sectionTypeId]})`;
        }

        nodes.push({
          id: String(sec.sectionId),
          title,
          nodeType: sec.sectionTypeId,
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
          {error?.message || 'Blend not found'}
        </Alert>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/blends')}
          sx={{ mt: 2 }}
        >
          Back to Blends
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
          code={data.code}
          blendId={blendId}
          viewMode={viewMode}
          isMobile={isMobile}
          isSidebarCollapsed={isSidebarCollapsed}
          onToggleSidebar={handleToggleClick}
          onBack={() => navigate('/blends')}
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
        <BlendEditor
          data={data}
          selectedSectionId={selectedSectionId}
          viewMode={viewMode}
          onSectionDeleted={() => setSelectedSection('header')}
        />
      </Box>
    </Box>
  );
};

// 2. Main Outer Component - Handles URL param parsing and invalid ID state
const BlendEditContent: React.FC = () => {
  const { id: rawId } = useParams<{ id: string }>();
  const navigate = useNavigate();

  // Validate and parse route parameter upfront
  const blendId = rawId ? parseInt(rawId, 10) : NaN;
  const isValidId = !isNaN(blendId) && blendId > 0;

  if (!isValidId) {
    return (
      <Container maxWidth="md" sx={{ py: 3 }}>
        <Paper variant="outlined" sx={{ p: 3 }}>
          <Typography variant="h6" color="error">
            Invalid Blend Identifier
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            The requested blend ID is missing or not a valid number.
          </Typography>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate('/blends')}
            sx={{ mt: 2 }}
          >
            Back to Blends
          </Button>
        </Paper>
      </Container>
    );
  }

  return <BlendEditContentInner blendId={blendId} />;
};

// Wrapper ensuring the provider is scoped specifically to this page
export const BlendEditPage: React.FC = () => (
  <BlendEditorProvider>
    <BlendEditContent />
  </BlendEditorProvider>
);