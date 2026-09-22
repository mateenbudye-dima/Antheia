// src/features/templates/pages/TemplateEditPage.tsx
import React, { useEffect, useState, useMemo, useCallback } from 'react';
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
import type { FullTemplateResponse } from '../types/template.types';
import { templatesApi } from '../api/templatesApi';
import { useLayout } from '../../../shared/layouts/LayoutContext';

export const TemplateEditPage: React.FC = () => {
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

  const [data, setData] = useState<FullTemplateResponse | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedSectionId, setSelectedSectionId] = useState<string>('header');
  const [viewMode, setViewMode] = useState<'split' | 'all'>('split');

  const templateId = useMemo(() => (id ? parseInt(id, 10) : null), [id]);

  // 1. Fetch Template Data
  useEffect(() => {
    let isMounted = true;

    const fetchTemplate = async () => {
      if (!templateId || isNaN(templateId)) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        const result = await templatesApi.getFullTemplate(templateId);
        if (isMounted) setData(result);
      } catch (err) {
        console.error('Error loading template:', err);
        if (isMounted) setError('Failed to load template data.');
      } finally {
        if (isMounted) setLoading(false);
      }
    };

    fetchTemplate();

    return () => {
      isMounted = false;
    };
  }, [templateId]);

  // 2. Build Tree Structure
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

  // 3. Handle Section Selection
  const handleSelectSection = useCallback(
    (secId: string) => {
      setSelectedSectionId(secId);
      if (isMobile) {
        toggleMobileSidebar();
      }
    },
    [isMobile, toggleMobileSidebar]
  );

  // 4. Inject Section Tree in Layout Sidebar
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

  // Handlers
  const handleViewModeChange = (
    _event: React.MouseEvent<HTMLElement>,
    newMode: 'split' | 'all' | null
  ) => {
    if (newMode !== null) {
      setViewMode(newMode);
    }
  };

  const handleToggleClick = () => {
    if (isMobile) {
      toggleMobileSidebar();
    } else {
      toggleSidebar();
    }
  };

  // --- Render Error & Loading States ---
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

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 6 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error || !data) {
    return (
      <Container maxWidth="md" sx={{ py: 3 }}>
        <Alert severity="error">{error || 'Template not found'}</Alert>
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
    <Box sx={{ width: '100%' }}>
      {/* Separated Header Toolbar Component */}
      <EditorHeader
        title={data.title}
        templateId={templateId}
        viewMode={viewMode}
        isMobile={isMobile}
        isSidebarCollapsed={isSidebarCollapsed}
        onToggleSidebar={handleToggleClick}
        onBack={() => navigate('/templates')}
        onViewModeChange={handleViewModeChange}
      />

      {/* Main Content Area */}
      <Paper
        variant="outlined"
        sx={{ p: { xs: 2, sm: 3 }, minHeight: 'calc(100vh - 160px)' }}
      >
        <TemplateEditor
          data={data}
          selectedSectionId={selectedSectionId}
          viewMode={viewMode}
        />
      </Paper>
    </Box>
  );
};