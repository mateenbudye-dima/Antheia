import React, { useCallback, useMemo } from 'react';
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
import { EditorToolbar } from '../components/EditorToolbar';
import { BlendEditor } from '../components/BlendEditor';
import { useBlend } from '../hooks/useBlend';
import { useTreeSections } from '../hooks/useTreeSections';
import { useBlendSidebar } from '../hooks/useBlendSidebar';
import { BlendEditorProvider } from '../context/BlendEditorContext';
import { useLayout } from '../../../shared/layouts/LayoutContext';
import { useBlendEditorContext } from '../hooks/useBlendEditorContext';
import { SectionTree } from '../components/SectionTree';

interface InnerProps {
  blendId: number;
}

const BlendEditContentInner: React.FC<InnerProps> = ({ blendId }) => {
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const { isSidebarCollapsed, toggleSidebar, toggleMobileSidebar } = useLayout();
  const { data, isLoading, isError, error } = useBlend(blendId);
  const { state, setSelectedSection, setViewMode } = useBlendEditorContext();
  const { selectedSectionId, viewMode } = state;

  // Custom Hooks for Data Transformation & Sidebar Operations
  const treeSections = useTreeSections(data);

  const handleSelectSection = useCallback(
    (secId: string) => {
      setSelectedSection(secId);
      if (isMobile) {
        toggleMobileSidebar();
      }
    },
    [isMobile, toggleMobileSidebar, setSelectedSection]
  );

  const sidebarContent = useMemo(
    () => (
      <SectionTree
        sections={treeSections}
        selectedSectionId={selectedSectionId}
        onSelectSection={handleSelectSection}
      />
    ),
    [treeSections, selectedSectionId, handleSelectSection]
  );

  useBlendSidebar(viewMode, sidebarContent);

  const handleToggleClick = () => {
    if (isMobile) {
      toggleMobileSidebar();
    } else {
      toggleSidebar();
    }
  };

  const handleSendForApproval = () => {
    // TODO: Connect send for approval handler/mutation
    console.log('Sending blend for approval:', blendId);
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
        <Alert severity="error">{error?.message || 'Blend not found'}</Alert>
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
      {/* Sticky Top Region (Header + Actions Toolbar) */}
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

        <EditorToolbar
          blendId={blendId}
          blendCode={data.code || ''}
          onActionSuccess={handleSendForApproval}
        />
      </Box>

      {/* Independently Scrollable Editor Content */}
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

// Route Parameter Validation Wrapper
const BlendEditContent: React.FC = () => {
  const { id: rawId } = useParams<{ id: string }>();
  const navigate = useNavigate();

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

export const BlendEditPage: React.FC = () => (
  <BlendEditorProvider>
    <BlendEditContent />
  </BlendEditorProvider>
);