import React from 'react';
import { Chip, CircularProgress } from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';

export type SaveStatus = 'idle' | 'saving' | 'saved' | 'error' | string;

interface StatusBadgeProps {
  status: SaveStatus;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status }) => {
  switch (status) {
    case 'saving':
      return (
        <Chip
          icon={<CircularProgress size={14} color="inherit" />}
          label="Saving..."
          size="small"
          color="warning"
          variant="outlined"
        />
      );
    case 'saved':
      return (
        <Chip
          icon={<CheckCircleIcon fontSize="small" />}
          label="Saved"
          size="small"
          color="success"
          variant="outlined"
        />
      );
    case 'error':
      return (
        <Chip
          icon={<ErrorIcon fontSize="small" />}
          label="Save Failed"
          size="small"
          color="error"
          variant="outlined"
        />
      );
    default:
      return null;
  }
};