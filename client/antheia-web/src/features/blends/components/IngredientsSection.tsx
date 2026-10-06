import React, { useState, useMemo } from 'react';
import { TableCell, Typography } from '@mui/material';
import { IngredientType, type Ingredient } from '../types/blend.types';
import { IngredientRow } from './IngredientRow';
import { useBlendMutations } from '../hooks/useBlendMutations';
import { BlendSectionTable, type ColumnConfig } from './shared/BlendSectionTable';

const COLUMNS: ColumnConfig[] = [
  { label: 'Name', width: '30%' },
  { label: 'Type', width: '25%' },
  { label: 'Ratio (%)', width: '15%' },
  { label: 'Qty (g)', width: '15%' },
  { label: 'Actions', width: '15%', align: 'center' },
];

interface Props {
  blendId: number;
  blendCode: string;
  sectionId: number;
  sectionTitle: string;
  initialIngredients: Ingredient[];
}

export const IngredientsSection: React.FC<Props> = ({
  blendId,
  blendCode,
  sectionId,
  sectionTitle,
  initialIngredients,
}) => {
  const [ingredients, setIngredients] = useState<Ingredient[]>(initialIngredients);
  const { addIngredientAsync, deleteIngredientAsync } = useBlendMutations(blendId);

  const totals = useMemo(() => {
    return ingredients.reduce(
      (acc, item) => ({
        totalRatio: acc.totalRatio + (Number(item.ratio) || 0),
        totalQuantity: acc.totalQuantity + (Number(item.quantity) || 0),
      }),
      { totalRatio: 0, totalQuantity: 0 }
    );
  }, [ingredients]);

  const addRow = async () => {
    try {
      const newIngredient = await addIngredientAsync({
        sectionId,
        name: 'New Ingredient',
        type: IngredientType.Unknown,
        ratio: 0,
        quantity: 0,
        SectionTitle: sectionTitle,
        BlendId: blendId,
        BlendCode: blendCode,
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

  const formatTotal = (val: number) => Number(val.toFixed(2));

  return (
    <BlendSectionTable
      columns={COLUMNS}
      addButtonLabel="Add Ingredient Row"
      onAddRow={addRow}
      footerNode={
        <>
          <TableCell colSpan={2}>
            <Typography variant="subtitle2" sx={{ fontWeight: 'bold' }} color="primary.dark">
              Total
            </Typography>
          </TableCell>
          <TableCell>
            <Typography variant="subtitle2" sx={{ fontWeight: 'bold' }} color="primary.dark">
              {formatTotal(totals.totalRatio)}%
            </Typography>
          </TableCell>
          <TableCell>
            <Typography variant="subtitle2" sx={{ fontWeight: 'bold' }} color="primary.dark">
              {formatTotal(totals.totalQuantity)} g
            </Typography>
          </TableCell>
          <TableCell />
        </>
      }
    >
      {ingredients.map((item) => (
        <IngredientRow
          key={item.sectionIngredientId}
          blendId={blendId}
          sectionId={sectionId}
          sectionTitle={sectionTitle}
          blendCode={blendCode}
          item={item}
          onChange={updateLocalField}
          onDelete={deleteRow}
        />
      ))}
    </BlendSectionTable>
  );
};