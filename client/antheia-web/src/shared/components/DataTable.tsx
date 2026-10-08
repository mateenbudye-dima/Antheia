import React from 'react';
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Skeleton,
  Typography,
  useTheme,
  type SxProps,
  type Theme,
} from '@mui/material';

export interface Column<T> {
  key: string;
  label: string;
  align?: 'left' | 'center' | 'right';
  width?: string | number;
  render?: (row: T, index: number) => React.ReactNode;
}

export interface DataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  getRowKey: (row: T, index: number) => string | number;
  isLoading?: boolean;
  skeletonRows?: number;
  emptyMessage?: React.ReactNode;
  onRowClick?: (row: T) => void;
  sx?: SxProps<Theme>;
}

export function DataTable<T>({
  columns,
  data,
  getRowKey,
  isLoading = false,
  skeletonRows = 5,
  emptyMessage = 'No data available.',
  onRowClick,
  sx,
}: DataTableProps<T>): React.ReactElement {
  const theme = useTheme();

  // Skeleton loading state
  if (isLoading) {
    return (
      <TableContainer
        component={Paper}
        variant="outlined"
        sx={{
          borderColor: theme.palette.divider,
          backgroundColor: theme.palette.background.paper,
          ...sx,
        }}
      >
        <Table size="medium">
          <TableHead>
            <TableRow sx={{ backgroundColor: theme.palette.action.hover }}>
              {columns.map((col) => (
                <TableCell
                  key={col.key}
                  align={col.align || 'left'}
                  style={{ width: col.width }}
                  sx={{ fontWeight: 'bold', color: theme.palette.text.primary }}
                >
                  {col.label}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {Array.from({ length: skeletonRows }).map((_, rowIndex) => (
              <TableRow key={rowIndex}>
                {columns.map((col) => (
                  <TableCell key={col.key} align={col.align || 'left'}>
                    <Skeleton variant="text" width="80%" />
                  </TableCell>
                ))}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    );
  }

  // Empty state
  if (data.length === 0) {
    return (
      <Paper
        variant="outlined"
        sx={{
          p: 6,
          textAlign: 'center',
          backgroundColor: theme.palette.background.paper,
          borderColor: theme.palette.divider,
          borderRadius: 2,
          ...sx,
        }}
      >
        {typeof emptyMessage === 'string' ? (
          <Typography color="text.secondary">{emptyMessage}</Typography>
        ) : (
          emptyMessage
        )}
      </Paper>
    );
  }

  return (
    <TableContainer
      component={Paper}
      variant="outlined"
      sx={{
        borderColor: theme.palette.divider,
        backgroundColor: theme.palette.background.paper,
        ...sx,
      }}
    >
      <Table size="medium">
        <TableHead>
          <TableRow
            sx={{
              backgroundColor:
                theme.palette.mode === 'dark'
                  ? 'rgba(255, 255, 255, 0.05)'
                  : theme.palette.action.hover,
            }}
          >
            {columns.map((col) => (
              <TableCell
                key={col.key}
                align={col.align || 'left'}
                style={{ width: col.width }}
                sx={{
                  fontWeight: 'bold',
                  color: theme.palette.text.primary,
                  borderColor: theme.palette.divider,
                }}
              >
                {col.label}
              </TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {data.map((row, index) => {
            const key = getRowKey(row, index);
            return (
              <TableRow
                key={key}
                hover
                onClick={() => onRowClick?.(row)}
                sx={{
                  cursor: onRowClick ? 'pointer' : 'default',
                  '&:last-child td, &:last-child th': { border: 0 },
                  '&:hover': {
                    backgroundColor: theme.palette.action.hover,
                  },
                }}
              >
                {columns.map((col) => {
                    const cellValue = col.render
                        ? col.render(row, index)
                        : (row as Record<string, unknown>)[col.key] as React.ReactNode;

                    return (
                        <TableCell
                        key={col.key}
                        align={col.align || 'left'}
                        sx={{ borderColor: theme.palette.divider }}
                        >
                        {cellValue}
                        </TableCell>
                    );
                })}
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </TableContainer>
  );
}