import React, { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { isAxiosError } from 'axios';
import {
  Box,
  Container,
  Typography,
  Button,
  Chip,
  Alert,
  CircularProgress,
  Tooltip,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { useCreateBlendDraft, useBlends } from '../hooks/useBlends';
import { BLEND_STATUS_LABELS, BlendStatus, getStatusColor } from '../types/blend.types';
import { useAuth } from '../../auth/hooks/useAuth';
import { DataTable, type Column } from '../../../shared/components/DataTable';
import type { BlendItem } from '../api/blendsApi';

export const BlendListPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();

  // 1. TanStack Query for reading blend list
  const { data: blends = [], isLoading, isError, error } = useBlends();

  // 2. TanStack Mutation for creating new blend draft
  const { mutate: createDraft, isPending: isCreating } = useCreateBlendDraft();

  const handleCreateNewBlend = () => {
    createDraft(undefined, {
      onSuccess: (data) => {
        navigate(`/blends/${data.blendId}/edit`);
      },
      onError: (err) => {
        const message = isAxiosError(err)
          ? err.response?.data?.message || 'Could not create blend draft.'
          : err instanceof Error
          ? err.message
          : 'Failed to create new draft';
        alert(message);
      },
    });
  };

  // Helper function to extract error message
  const getErrorMessage = () => {
    if (!error) return null;
    if (isAxiosError(error)) {
      return error.response?.data?.message || 'Failed to load blends.';
    }
    if (error instanceof Error) {
      return error.message;
    }
    return 'An unexpected error occurred.';
  };

  // Table Columns Definition
  const columns: Column<BlendItem>[] = useMemo(
    () => [
      {
        key: 'code',
        label: 'Blend Code',
        render: (blend) => (
          <Typography variant="body2" sx={{ fontWeight: 600 }}>
            {blend.code || 'Untitled Blend'}
          </Typography>
        ),
      },
      {
        key: 'trialNumber',
        label: 'Trial',
        render: (blend) => blend.trialNumber ?? 'N/A',
      },
      {
        key: 'status',
        label: 'Status',
        render: (blend) => (
          <Chip
            label={
              blend?.status !== undefined
                ? BLEND_STATUS_LABELS[blend.status as BlendStatus]
                : 'Draft'
            }
            color={getStatusColor(blend.status as BlendStatus)}
            size="small"
            variant="outlined"
            sx={{ fontWeight: 'bold' }}
          />
        ),
      },
      {
        key: 'objective',
        label: 'Objective',
        render: (blend) => (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              maxWidth: 300,
              whiteSpace: 'nowrap',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
            }}
          >
            {blend.objective || 'No objective provided.'}
          </Typography>
        ),
      },
      {
        key: 'updatedDate',
        label: 'Updated Date',
        render: (blend) => new Date(blend.updatedDate).toLocaleDateString(),
      },
      {
        key: 'actions',
        label: 'Actions',
        align: 'right',
        render: (blend) => {
          const isOwner = user?.userId === blend.createdBy;
          const isEditable = isOwner && blend.status === BlendStatus.Draft;

          return isEditable ? (
            <Tooltip title="Edit Blend">
              <Button
                variant="outlined"
                size="small"
                color="primary"
                startIcon={<EditIcon />}
                onClick={() => navigate(`/blends/${blend.blendId}/edit`)}
              >
                Edit
              </Button>
            </Tooltip>
          ) : (
            <Tooltip title="View Blend">
              <Button
                variant="outlined"
                size="small"
                color="secondary"
                startIcon={<VisibilityIcon />}
                onClick={() => navigate(`/blends/${blend.blendId}/details`)}
              >
                View
              </Button>
            </Tooltip>
          );
        },
      },
    ],
    [user?.userId, navigate]
  );

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {/* Header Section */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 4,
          flexWrap: 'wrap',
          gap: 2,
        }}
      >
        <Box>
          <Typography
            variant="h4"
            component="h1"
            sx={{ fontWeight: 'bold' }}
            gutterBottom
          >
            Blends
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Manage and edit your platform product blend specifications.
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={
            isCreating ? <CircularProgress size={20} color="inherit" /> : <AddIcon />
          }
          onClick={handleCreateNewBlend}
          disabled={isCreating}
          size="large"
        >
          {isCreating ? 'Initializing Draft...' : 'Create New Blend'}
        </Button>
      </Box>

      {/* Error Alert */}
      {isError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {getErrorMessage()}
        </Alert>
      )}

      {/* Generic Display Data Table */}
      <DataTable<BlendItem>
        columns={columns}
        data={blends}
        isLoading={isLoading}
        getRowKey={(blend) => blend.blendId}
        emptyMessage={
          <Typography color="text.secondary">
            No blends found. Click <strong>"+ Create New Blend"</strong> to start a new draft.
          </Typography>
        }
      />
    </Container>
  );
};