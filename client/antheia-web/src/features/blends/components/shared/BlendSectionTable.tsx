import React from 'react';
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableFooter,
  Button,
  Box,
  alpha,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';

export interface ColumnConfig {
  label: string;
  width?: string | number;
  align?: 'left' | 'center' | 'right';
}

interface BlendSectionTableProps {
  columns: ColumnConfig[];
  children: React.ReactNode; // The <TableBody> rows
  addButtonLabel: string;
  onAddRow: () => void;
  readOnly?: boolean;
  footerNode?: React.ReactNode; // Optional footer/totals row
}

export const BlendSectionTable: React.FC<BlendSectionTableProps> = ({
  columns,
  children,
  addButtonLabel,
  onAddRow,
  readOnly = false,
  footerNode,
}) => {
  return (
    <Paper
      variant="outlined"
      sx={{
        p: 2,
        mt: 3,
        borderColor: 'primary.light',
      }}
    >
      <TableContainer>
        <Table size="small">
          {/* Unified Primary Header */}
          <TableHead>
            <TableRow
              sx={{
                backgroundColor: 'primary.main',
                '& .MuiTableCell-head': {
                  color: 'primary.contrastText',
                  fontWeight: 'bold',
                },
              }}
            >
              {columns
                .filter((col) => !readOnly || col.label !== 'Actions')
                .map((col, idx) => (
                <TableCell
                  key={idx}
                  align={col.align ?? 'left'}
                  style={{ width: col.width }}
                >
                  {col.label}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>

          <TableBody>{children}</TableBody>

          {/* Optional Styled Footer */}
          {footerNode && (
            <TableFooter>
              <TableRow
                sx={{
                  backgroundColor: (theme) => alpha(theme.palette.primary.main, 0.08),
                  borderTop: (theme) => `2px solid ${theme.palette.primary.main}`,
                  '& .MuiTableCell-root': { py: 1.5 },
                }}
              >
                {footerNode}
              </TableRow>
            </TableFooter>
          )}
        </Table>
      </TableContainer>

      {!readOnly && (
        <Box sx={{ mt: 2 }}>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={onAddRow}
            size="small"
          >
            {addButtonLabel}
          </Button>
        </Box>
      )}
    </Paper>
  );
};