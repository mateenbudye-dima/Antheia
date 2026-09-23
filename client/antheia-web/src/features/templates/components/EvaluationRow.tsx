import React from 'react';
import { TableRow, TableCell, TextField, Select, MenuItem, IconButton, Tooltip } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import type { EvaluationItem } from '../types/template.types';
import { useAutoSave } from '../../../shared/hooks/useAutoSave';
import { useTemplateMutations } from '../hooks/useTemplateMutations';

interface EvaluationRowProps {
  templateId: number;
  item: EvaluationItem;
  onChange: (id: number, field: keyof EvaluationItem, value: string) => void;
  onDelete: (id: number) => void;
}

export const EvaluationRow: React.FC<EvaluationRowProps> = ({ templateId, item, onChange, onDelete }) => {
  const { updateEvaluationAsync } = useTemplateMutations(templateId);
    
  useAutoSave({
      value: item,
      delay: 800,
      onSave: async (debounced) => {
        await updateEvaluationAsync({
          evaluationId:debounced.evaluationId, 
          payload: {
          evaluationParameter: debounced.evaluationParameter,
          specification: debounced.specification,
          result: debounced.result,
          status: debounced.status,
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
          value={item.evaluationParameter ?? ''}
          onChange={(e) => onChange(item.evaluationId, 'evaluationParameter', e.target.value)}
        />
      </TableCell>
      <TableCell>
        <TextField
          size="small"
          fullWidth
          value={item.result ?? ''}
          onChange={(e) => onChange(item.evaluationId, 'result', e.target.value)}
        />
      </TableCell>
      <TableCell>
        <TextField
          size="small"
          fullWidth
          value={item.specification ?? ''}
          onChange={(e) => onChange(item.evaluationId, 'specification', e.target.value)}
        />
      </TableCell>
      <TableCell>
        <Select
          size="small"
          fullWidth
          value={item.status ?? 'Pending'}
          onChange={(e) => onChange(item.evaluationId, 'status', e.target.value)}
        >
          <MenuItem value="Pending">Pending</MenuItem>
          <MenuItem value="Pass">Pass</MenuItem>
          <MenuItem value="Fail">Fail</MenuItem>
        </Select>
      </TableCell>
      <TableCell align="center">
        <Tooltip title="Delete parameter">
          <IconButton
            color="error"
            size="small"
            onClick={() => onDelete(item.evaluationId)}
          >
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </TableCell>
    </TableRow>
  );
};