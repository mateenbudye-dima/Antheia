import React from 'react';
import {
  TableRow,
  TableCell,
  TextField,
  IconButton,
  Tooltip,
  Select,
  MenuItem,
  FormControl,
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import {
  IngredientType,
  INGREDIENT_TYPE_OPTIONS,
  type Ingredient,
} from '../types/blend.types';
import { useAutoSave } from '../../../shared/hooks/useAutoSave';
import { useBlendMutations } from '../hooks/useBlendMutations';

interface IngredientRowProps {
  blendId: number;
  sectionId: number;
  sectionTitle: string;
  blendCode: string;
  item: Ingredient;
  onChange: (id: number, field: keyof Ingredient, value: string | number) => void;
  onDelete: (id: number) => void;

  
}

export const IngredientRow: React.FC<IngredientRowProps> = ({
  blendId,
  sectionId,
  sectionTitle,
  blendCode,
  item,
  onChange,
  onDelete,
}) => {
  const { updateIngredientAsync } = useBlendMutations(blendId);

  useAutoSave({
    value: item,
    delay: 800,
    onSave: async (debouncedItem) => {
      await updateIngredientAsync({
        id: debouncedItem.sectionIngredientId,
        payload: {
          name: debouncedItem.name,
          type: debouncedItem.type,
          ratio: debouncedItem.ratio,
          quantity: debouncedItem.quantity,
          SectionId: sectionId, 
          SectionTitle: sectionTitle, 
          BlendId: blendId,
          BlendCode: blendCode, 
        },
      });
    }
  });

  // Block non-numeric key presses ('e', 'E', '+', '-') in number fields
  const handleNumberKeyDown = (e: React.KeyboardEvent) => {
    if (['e', 'E', '+', '-'].includes(e.key)) {
      e.preventDefault();
    }
  };

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
        <FormControl fullWidth size="small">
          <Select
            value={item.type ?? IngredientType.Unknown}
            onChange={(e) =>
              onChange(
                item.sectionIngredientId,
                'type',
                Number(e.target.value) as IngredientType
              )
            }
          >
            {INGREDIENT_TYPE_OPTIONS.map((option) => (
              <MenuItem key={option.value} value={option.value}>
                {option.label}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </TableCell>
      <TableCell>
        <TextField
          size="small"
          type="number"
          fullWidth
          value={item.ratio ?? ''}
          onKeyDown={handleNumberKeyDown}
          slotProps={{ htmlInput: { min: 0, step: 'any' } }}
          onChange={(e) => {
            const val = e.target.value;
            onChange(
              item.sectionIngredientId,
              'ratio',
              val === '' ? 0 : parseFloat(val) || 0
            );
          }}
        />
      </TableCell>
      <TableCell>
        <TextField
          size="small"
          type="number"
          fullWidth
          value={item.quantity ?? ''}
          onKeyDown={handleNumberKeyDown}
          slotProps={{ htmlInput: { min: 0, step: 'any' } }}
          onChange={(e) => {
            const val = e.target.value;
            onChange(
              item.sectionIngredientId,
              'quantity',
              val === '' ? 0 : parseFloat(val) || 0
            );
          }}
        />
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