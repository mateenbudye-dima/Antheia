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
import { templatesApi } from '../api/templatesApi';
import { EvaluationRow } from './EvaluationRow';

interface Props {
  sectionId: number;
  initialEvaluations: EvaluationItem[];
}

export const EvaluationSection: React.FC<Props> = ({ sectionId, initialEvaluations }) => {
  const [evaluations, setEvaluations] = useState<EvaluationItem[]>(initialEvaluations);

  const addParam = async () => {
    try {
      const newEvaluation = await templatesApi.addEvaluation({
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
      await templatesApi.deleteEvaluation(id);
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
              <TableCell align="center" style={{ width: '10%' }}>Auto Save</TableCell>
              <TableCell align="center" style={{ width: '5%' }}>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {evaluations.map((item) => (
              <EvaluationRow
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