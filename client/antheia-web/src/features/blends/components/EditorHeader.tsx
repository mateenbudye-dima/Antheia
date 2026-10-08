import React from 'react';
import {
  Box,
  Typography,
  Tooltip,
  ToggleButton,
  ToggleButtonGroup,
  Chip,
} from '@mui/material';
import ViewListIcon from '@mui/icons-material/ViewList';
import AccountTreeIcon from '@mui/icons-material/AccountTree';
import { StatusBadge } from '../../../shared/components/StatusBadge';
import { useBlendMutations } from '../hooks/useBlendMutations';
import { BLEND_STATUS_LABELS, BlendStatus, getStatusColor } from '../types/blend.types';

export interface EditorHeaderProps {
  code?: string;
  blendId: number;
  blendStatus: BlendStatus;
  readOnly?: boolean;
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
  code,
  blendId,
  blendStatus,
  readOnly = false,
  viewMode,
  onViewModeChange,
}) => {
  // Read aggregated saveStatus across ALL header/ingredient/prep/eval mutations
  const { saveStatus } = useBlendMutations(blendId);

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

      {/* Center: Code + Global Status Badge */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
        <Typography variant="h6" sx={{ fontWeight: 'bold' }}>
          {readOnly
            ? code ? `Blend Details: ${code}` : `Blend #${blendId} Details`
            : code ? `Editing: ${code}` : `Editing Blend #${blendId}`}
        </Typography>
        {!readOnly && <StatusBadge status={saveStatus} />}
      </Box>

      {/* Blend Status */}
      <Chip
        label={blendStatus !== undefined ? BLEND_STATUS_LABELS[blendStatus] : 'Draft'}
        color={getStatusColor(blendStatus as BlendStatus)}
        size="small"
        variant="outlined"
        sx={{ fontWeight: 'bold', ml: 'auto' }}
      />

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
