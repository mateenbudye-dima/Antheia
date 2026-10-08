import React, { useCallback, useEffect, useMemo } from 'react';
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
import { SectionTree } from '../components/SectionTree';

import { useBlend } from '../hooks/useBlend';
import { useTreeSections } from '../hooks/useTreeSections';
import { useBlendSidebar } from '../hooks/useBlendSidebar';
import { useBlendEditorContext } from '../hooks/useBlendEditorContext';
import { BlendEditorProvider } from '../context/BlendEditorContext';
import { useLayout } from '../../../shared/layouts/LayoutContext';
import { useAuth } from '../../auth/hooks/useAuth';
import { BlendStatus } from '../types/blend.types';

interface InnerProps {
  blendId: number;
  readOnly: boolean;
}

const BlendEditContentInner: React.FC<InnerProps> = ({ blendId, readOnly }) => {
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  // 1. Data & Context Queries
  const { user, isLoading: isAuthLoading } = useAuth();
  const { data, isLoading: isBlendLoading, isError, error } = useBlend(blendId);
  const isDataLoaded = !isAuthLoading && !isBlendLoading;

  const { isSidebarCollapsed, toggleSidebar, toggleMobileSidebar } = useLayout();
  const { state, setSelectedSection, setViewMode } = useBlendEditorContext();
  const { selectedSectionId, viewMode } = state;

  const treeSections = useTreeSections(data);

  // 2. Authorization & Status Evaluation
  const isAuthor = useMemo(() => {
    if (!data?.createdBy || !user?.userId) return false;
    return String(data.createdBy) === String(user.userId);
  }, [data?.createdBy, user?.userId]);

  const isDraft = data?.status === BlendStatus.Draft;
  const canEdit = readOnly ? false : isAuthor && isDraft;

  // 3. Authorization Redirect Effect
  useEffect(() => {
    // Only check redirect once ALL data (auth + blend) has finished loading
    if (isDataLoaded && data && !readOnly && !canEdit) {
      navigate(`/blends/${blendId}/details`, { replace: true });
    }
  }, [isDataLoaded, data, readOnly, canEdit, blendId, navigate]);

  // 4. Sidebar Callbacks & Registration
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

  // Sync sidebar only when data is ready
  useBlendSidebar(isDataLoaded ? viewMode : 'all', sidebarContent);

  const handleToggleClick = () => {
    if (isMobile) {
      toggleMobileSidebar();
    } else {
      toggleSidebar();
    }
  };

  const handleSendForApproval = () => {
    console.log('Sending blend for approval:', blendId);
  };

  // 5. Early Return Guards (Loading & Error States)
  if (!isDataLoaded || (!readOnly && !canEdit)) {
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

  // 6. Main UI Render
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
      {/* Sticky Top Region */}
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
          blendStatus={data.status as BlendStatus}
          readOnly={readOnly}
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
          blendStatus={data.status as BlendStatus}
          onActionSuccess={handleSendForApproval}
          readOnly={readOnly}
          isAuthor={isAuthor}
        />
      </Box>

      {/* Main Scrollable Content Area */}
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
          readOnly={readOnly}
          onSectionDeleted={() => setSelectedSection('header')}
        />
      </Box>
    </Box>
  );
};

// Route Parameter Validation Wrapper
const BlendContent: React.FC<{ readOnly: boolean }> = ({ readOnly }) => {
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

  return <BlendEditContentInner blendId={blendId} readOnly={readOnly} />;
};

export const BlendEditPage: React.FC<{ readOnly: boolean }> = ({ readOnly }) => (
  <BlendEditorProvider>
    <BlendContent readOnly={readOnly} />
  </BlendEditorProvider>
);