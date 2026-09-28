import React, { useState } from 'react';
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  Box,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import type { Ingredient } from '../types/blend.types';
import { IngredientRow } from './IngredientRow';
import { useBlendMutations } from '../hooks/useBlendMutations';

interface Props {
  blendId: number;
  sectionId: number;
  initialIngredients: Ingredient[];
}

export const IngredientsSection: React.FC<Props> = ({ blendId, sectionId, initialIngredients }) => {
  const [ingredients, setIngredients] = useState<Ingredient[]>(initialIngredients);

  const { addIngredientAsync, deleteIngredientAsync } = useBlendMutations(blendId);

  const addRow = async () => {
    try {
      const newIngredient = await addIngredientAsync({
        sectionId,
        name: 'New Ingredient',
        type: 'Active',
        ratio: 0,
        quantity: 0,
      });
      setIngredients((prev) => [...prev, newIngredient]);
    } catch (err) {
      console.error('Failed to add ingredient:', err);
    }
  };

  const updateLocalField = (id: number, field: keyof Ingredient, value: string | number) => {
    setIngredients((prev) =>
      prev.map((item) => (item.sectionIngredientId === id ? { ...item, [field]: value } : item))
    );
  };

  const deleteRow = async (id: number) => {
    try {
      await deleteIngredientAsync(id);
      setIngredients((prev) => prev.filter((item) => item.sectionIngredientId !== id));
    } catch (err) {
      console.error('Failed to delete ingredient:', err);
    }
  };

  return (
    <Paper sx={{ p: 2, mt: 3 }} variant="outlined">      
      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell style={{ width: '30%' }}>Name</TableCell>
              <TableCell style={{ width: '25%' }}>Type</TableCell>
              <TableCell style={{ width: '15%' }}>Ratio (%)</TableCell>
              <TableCell style={{ width: '15%' }}>Qty (g)</TableCell>
              <TableCell align="center" style={{ width: '5%' }}>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {ingredients.map((item) => (
              <IngredientRow
                key={item.sectionIngredientId}
                blendId={blendId}
                item={item}
                onChange={updateLocalField}
                onDelete={deleteRow}
              />
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      <Box sx={{ mt: 2 }}>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={addRow}
          size="small"
        >
          Add Ingredient Row
        </Button>
      </Box>
    </Paper>
  );
};