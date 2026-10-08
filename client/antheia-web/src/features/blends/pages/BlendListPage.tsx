import React, { useMemo, useState } from 'react';
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
  TablePagination,
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
import { BlendListToolbar, type PublicationFilter } from '../components/BlendListToolbar';
import { useDebounce } from '../../../shared/hooks/useDebounce';

export const BlendListPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();

  const [searchQuery, setSearchQuery] = useState('');
  const [selectedStatuses, setSelectedStatuses] = useState<BlendStatus[]>([]);
  const [publicationFilter, setPublicationFilter] = useState<PublicationFilter>('all');
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const debouncedSearchQuery = useDebounce(searchQuery.trim(), 350);
  const filters = useMemo(
    () => ({
      search: debouncedSearchQuery || undefined,
      statuses: selectedStatuses.length > 0 ? [...selectedStatuses].sort((a, b) => a - b) : undefined,
      isPublished:
        publicationFilter === 'all' ? undefined : publicationFilter === 'published',
      page: page + 1,
      pageSize,
    }),
    [debouncedSearchQuery, selectedStatuses, publicationFilter, page, pageSize]
  );

  // Fetch only the blends matching the current server-side filters.
  const { data, isLoading, isError, error } = useBlends(filters);
  const blends = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;

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
      <Box
        sx={{
          display: 'flex',
          flexDirection: { xs: 'column', sm: 'row' },
          alignItems: 'flex-start',
          gap: 2,
          mb: 2,
        }}
      >
        <Box sx={{ flex: 1, minWidth: 0 }}>
          <BlendListToolbar
            searchQuery={searchQuery}
            selectedStatuses={selectedStatuses}
            publicationFilter={publicationFilter}
            onSearchChange={(value) => {
              setSearchQuery(value);
              setPage(0);
            }}
            onStatusesChange={(value) => {
              setSelectedStatuses(value);
              setPage(0);
            }}
            onPublicationFilterChange={(value) => {
              setPublicationFilter(value);
              setPage(0);
            }}
            onClearAll={() => {
              setSearchQuery('');
              setSelectedStatuses([]);
              setPublicationFilter('all');
              setPage(0);
            }}
          />
        </Box>
        <Button
          variant="contained"
          startIcon={
            isCreating ? <CircularProgress size={20} color="inherit" /> : <AddIcon />
          }
          onClick={handleCreateNewBlend}
          disabled={isCreating}
          size="large"
          sx={{ flexShrink: 0 }}
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
            {searchQuery.trim() || selectedStatuses.length > 0 || publicationFilter !== 'all'
              ? 'No blends match your search or filters. Clear them to see all blends.'
              : <>No blends found. Click <strong>"+ Create New Blend"</strong> to start a new draft.</>}
          </Typography>
        }
      />
      <TablePagination
        component="div"
        count={totalCount}
        page={page}
        onPageChange={(_, nextPage) => setPage(nextPage)}
        rowsPerPage={pageSize}
        onRowsPerPageChange={(event) => {
          setPageSize(Number(event.target.value));
          setPage(0);
        }}
        rowsPerPageOptions={[10, 25, 50]}
        labelRowsPerPage="Blends per page:"
        sx={{ borderTop: 1, borderColor: 'divider' }}
      />
    </Container>
  );
};