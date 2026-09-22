import React from 'react';
import { TableRow, TableCell, TextField, IconButton, Tooltip } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import type { Ingredient } from '../types/template.types';
import { useAutoSave } from '../../../shared/hooks/useAutoSave';
import { StatusBadge } from '../../../shared/components/StatusBadge';
import { useTemplateMutations } from '../hooks/useTemplateMutations';

interface IngredientRowProps {
  templateId: number;
  item: Ingredient;
  onChange: (id: number, field: keyof Ingredient, value: string | number) => void;
  onDelete: (id: number) => void;
}

export const IngredientRow: React.FC<IngredientRowProps> = ({templateId, item, onChange, onDelete }) => {
  const { updateIngredientAsync } = useTemplateMutations(templateId);
  
  const { status } = useAutoSave({
    value: item,
    delay: 800,
    onSave: async (debouncedItem) => {
      await updateIngredientAsync({
        id:debouncedItem.sectionIngredientId, 
        payload:{
          name: debouncedItem.name,
          type: debouncedItem.type,
          ratio: debouncedItem.ratio,
          quantity: debouncedItem.quantity,
        }
      });
    },
  });

  return (
    <TableRow hover>
      <TableCell>
        <TextField
          size="small"
          fullWidth
          value={item.name ?? ''}
          onChange={(e) => onChange(item.sectionIngredientId, 'name', e.target.value)}
        />
      </TableCell>
      <TableCell>
        <TextField
          size="small"
          fullWidth
          value={item.type ?? ''}
          onChange={(e) => onChange(item.sectionIngredientId, 'type', e.target.value)}
        />
      </TableCell>
      <TableCell>
        <TextField
          size="small"
          type="number"
          fullWidth
          value={item.ratio ?? ''}
          onChange={(e) => onChange(item.sectionIngredientId, 'ratio', parseFloat(e.target.value) || 0)}
        />
      </TableCell>
      <TableCell>
        <TextField
          size="small"
          type="number"
          fullWidth
          value={item.quantity ?? ''}
          onChange={(e) => onChange(item.sectionIngredientId, 'quantity', parseFloat(e.target.value) || 0)}
        />
      </TableCell>
      <TableCell align="center">
        <StatusBadge status={status} />
      </TableCell>
      <TableCell align="center">
        <Tooltip title="Delete row">
          <IconButton
            color="error"
            size="small"
            onClick={() => onDelete(item.sectionIngredientId)}
          >
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </TableCell>
    </TableRow>
  );
};