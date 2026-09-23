import React from 'react';
import {
  Box,
  Button,
  Typography,
  Tooltip,
  ToggleButton,
  ToggleButtonGroup,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import ViewListIcon from '@mui/icons-material/ViewList';
import AccountTreeIcon from '@mui/icons-material/AccountTree';
import { StatusBadge } from '../../../shared/components/StatusBadge';
import { useTemplateMutations } from '../hooks/useTemplateMutations';

export interface EditorHeaderProps {
  title?: string;
  templateId: number;
  viewMode: 'split' | 'all';
  isMobile: boolean;
  isSidebarCollapsed: boolean;
  onToggleSidebar: () => void;
  onBack: () => void;
  onViewModeChange: (
    event: React.MouseEvent<HTMLElement>,
    newMode: 'split' | 'all' | null
  ) => void;
}

export const EditorHeader: React.FC<EditorHeaderProps> = ({
  title,
  templateId,
  viewMode,
  onBack,
  onViewModeChange,
}) => {
  // Read aggregated saveStatus across ALL header/ingredient/prep/eval mutations
  const { saveStatus } = useTemplateMutations(templateId);

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        flexWrap: 'wrap',
        gap: 2,
      }}
    >
      {/* Left: Section Tree Menu Toggle + Back Button */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={onBack} size="small">
          Back to Templates
        </Button>
      </Box>

      {/* Center: Title + Global Status Badge */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
        <Typography variant="h6" sx={{ fontWeight: 'bold' }}>
          {title ? `Editing: ${title}` : `Editing Template #${templateId}`}
        </Typography>
        <StatusBadge status={saveStatus} />
      </Box>

      {/* Right: View Mode Toggle */}
      <ToggleButtonGroup
        value={viewMode}
        exclusive
        onChange={onViewModeChange}
        size="small"
        aria-label="editor view mode"
      >
        <ToggleButton value="split" aria-label="tree view">
          <Tooltip title="Tree / Section View">
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <AccountTreeIcon fontSize="small" />
              <Typography
                variant="caption"
                sx={{
                  textTransform: 'capitalize',
                  display: { xs: 'none', sm: 'inline' },
                }}
              >
                Tree View
              </Typography>
            </Box>
          </Tooltip>
        </ToggleButton>

        <ToggleButton value="all" aria-label="complete view">
          <Tooltip title="Complete Stacked View">
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <ViewListIcon fontSize="small" />
              <Typography
                variant="caption"
                sx={{
                  textTransform: 'capitalize',
                  display: { xs: 'none', sm: 'inline' },
                }}
              >
                Complete View
              </Typography>
            </Box>
          </Tooltip>
        </ToggleButton>
      </ToggleButtonGroup>
    </Box>
  );
};