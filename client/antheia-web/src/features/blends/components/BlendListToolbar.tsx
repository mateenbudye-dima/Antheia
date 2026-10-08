import React, { useState } from 'react';
import {
  Box,
  Button,
  Checkbox,
  Chip,
  FormControl,
  FormControlLabel,
  FormGroup,
  FormLabel,
  IconButton,
  InputAdornment,
  MenuItem,
  Popover,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import FilterListIcon from '@mui/icons-material/FilterList';
import SearchIcon from '@mui/icons-material/Search';
import CloseIcon from '@mui/icons-material/Close';
import ClearIcon from '@mui/icons-material/Clear';
import { BLEND_STATUS_LABELS, BlendStatus } from '../types/blend.types';

export type PublicationFilter = 'all' | 'published' | 'unpublished';

interface BlendListToolbarProps {
  searchQuery: string;
  selectedStatuses: BlendStatus[];
  publicationFilter: PublicationFilter;
  onSearchChange: (value: string) => void;
  onStatusesChange: (value: BlendStatus[]) => void;
  onPublicationFilterChange: (value: PublicationFilter) => void;
  onClearAll: () => void;
}

const statusOptions = Object.values(BlendStatus) as BlendStatus[];

export const BlendListToolbar: React.FC<BlendListToolbarProps> = ({
  searchQuery,
  selectedStatuses,
  publicationFilter,
  onSearchChange,
  onStatusesChange,
  onPublicationFilterChange,
  onClearAll,
}) => {
  const [filterAnchor, setFilterAnchor] = useState<HTMLButtonElement | null>(null);
  const filterCount = selectedStatuses.length + (publicationFilter === 'all' ? 0 : 1);
  const hasActiveFilters = searchQuery.length > 0 || filterCount > 0;

  const toggleStatus = (status: BlendStatus) => {
    onStatusesChange(
      selectedStatuses.includes(status)
        ? selectedStatuses.filter((selectedStatus) => selectedStatus !== status)
        : [...selectedStatuses, status]
    );
  };

  return (
    <Box>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5}>
        <TextField
          fullWidth
          size="small"
          placeholder="Search blends by code, trial, or objective"
          value={searchQuery}
          onChange={(event) => onSearchChange(event.target.value)}
          slotProps={{
            htmlInput: { 'aria-label': 'Search blends' },
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon color="action" />
                </InputAdornment>
              ),
              endAdornment: searchQuery ? (
                <InputAdornment position="end">
                  <IconButton
                    aria-label="Clear search"
                    size="small"
                    onClick={() => onSearchChange('')}
                    edge="end"
                  >
                    <CloseIcon fontSize="small" />
                  </IconButton>
                </InputAdornment>
              ) : undefined,
            },
          }}
        />
        <Button
          variant="outlined"
          startIcon={<FilterListIcon />}
          onClick={(event) => setFilterAnchor(event.currentTarget)}
          aria-haspopup="true"
          aria-expanded={Boolean(filterAnchor)}
          sx={{ minWidth: { sm: 132 }, flexShrink: 0 }}
        >
          Filters{filterCount > 0 ? ` (${filterCount})` : ''}
        </Button>
      </Stack>

      {hasActiveFilters && (
        <Stack
          direction="row"
          spacing={1}
          useFlexGap
          sx={{ mt: 1.5, flexWrap: 'wrap', alignItems: 'center' }}
        >
          <Typography variant="body2" color="text.secondary">
            Active:
          </Typography>
          {searchQuery && (
            <Chip
              size="small"
              label={`Search: ${searchQuery}`}
              onDelete={() => onSearchChange('')}
            />
          )}
          {selectedStatuses.map((status) => (
            <Chip
              key={status}
              size="small"
              label={`Status: ${BLEND_STATUS_LABELS[status]}`}
              onDelete={() => toggleStatus(status)}
            />
          ))}
          {publicationFilter !== 'all' && (
            <Chip
              size="small"
              label={`Publication: ${publicationFilter === 'published' ? 'Published' : 'Unpublished'}`}
              onDelete={() => onPublicationFilterChange('all')}
            />
          )}
          <Button
            size="small"
            color="inherit"
            startIcon={<ClearIcon />}
            onClick={onClearAll}
          >
            Clear all
          </Button>
        </Stack>
      )}

      <Popover
        open={Boolean(filterAnchor)}
        anchorEl={filterAnchor}
        onClose={() => setFilterAnchor(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        <Box sx={{ p: 2.5, width: 280 }}>
          <FormControl fullWidth size="small" sx={{ mb: 2.5 }}>
            <FormLabel sx={{ mb: 1, color: 'text.primary', typography: 'subtitle2' }}>
              Publication
            </FormLabel>
            <Select
              value={publicationFilter}
              onChange={(event) =>
                onPublicationFilterChange(event.target.value as PublicationFilter)
              }
              inputProps={{ 'aria-label': 'Filter by publication' }}
            >
              <MenuItem value="all">Any publication state</MenuItem>
              <MenuItem value="published">Published</MenuItem>
              <MenuItem value="unpublished">Unpublished</MenuItem>
            </Select>
          </FormControl>
          <FormControl component="fieldset" fullWidth>
            <FormLabel
              component="legend"
              sx={{ mb: 0.5, color: 'text.primary', typography: 'subtitle2' }}
            >
              Status
            </FormLabel>
            <FormGroup>
              {statusOptions.map((status) => (
                <FormControlLabel
                  key={status}
                  label={BLEND_STATUS_LABELS[status]}
                  control={
                    <Checkbox
                      size="small"
                      checked={selectedStatuses.includes(status)}
                      onChange={() => toggleStatus(status)}
                    />
                  }
                />
              ))}
            </FormGroup>
          </FormControl>
        </Box>
      </Popover>
    </Box>
  );
};
