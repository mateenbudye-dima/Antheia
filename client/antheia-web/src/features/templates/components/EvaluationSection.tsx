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
  Typography,
  Box,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import type { EvaluationItem } from '../types/template.types';
import { EvaluationRow } from './EvaluationRow';
import { useTemplateMutations } from '../hooks/useTemplateMutations';

interface Props {
  templateId: number;
  sectionId: number;
  initialEvaluations: EvaluationItem[];
}

export const EvaluationSection: React.FC<Props> = ({ templateId, sectionId, initialEvaluations }) => {
  const [evaluations, setEvaluations] = useState<EvaluationItem[]>(initialEvaluations);

  const { addEvaluationAsync, deleteEvaluationAsync } = useTemplateMutations(templateId);

  const addParam = async () => {
    try {
      const newEvaluation = await addEvaluationAsync({
        sectionId,
        evaluationParameterType: 1,
        specification: 'Parameter Name',
        result: '',
        status: 'Pending',
      });
      setEvaluations((prev) => [...prev, newEvaluation]);
    } catch (err) {
      console.error('Failed adding evaluation:', err);
    }
  };

  const updateField = (id: number, field: keyof EvaluationItem, value: string) => {
    setEvaluations((prev) =>
      prev.map((item) => (item.evaluationId === id ? { ...item, [field]: value } : item))
    );
  };

  const deleteRow = async (id: number) => {
    try {
      await deleteEvaluationAsync(id);
      setEvaluations((prev) => prev.filter((item) => item.evaluationId !== id));
    } catch (err) {
      console.error('Failed deleting evaluation row:', err);
    }
  };

  return (
    <Paper sx={{ p: 2, mt: 3 }} variant="outlined">
      <Typography variant="h6" gutterBottom>
        3. Evaluation Parameters & Results
      </Typography>
      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell style={{ width: '40%' }}>Specification / Parameter</TableCell>
              <TableCell style={{ width: '30%' }}>Result</TableCell>
              <TableCell style={{ width: '15%' }}>Status</TableCell>
              <TableCell align="center" style={{ width: '5%' }}>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {evaluations.map((item) => (
              <EvaluationRow
                templateId={templateId}
                key={item.evaluationId}
                item={item}
                onChange={updateField}
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
          onClick={addParam}
          size="small"
        >
          Add Evaluation Parameter
        </Button>
      </Box>
    </Paper>
  );
};