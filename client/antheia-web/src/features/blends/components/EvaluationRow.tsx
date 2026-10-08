import React from 'react';
import { TableRow, TableCell, TextField, Select, MenuItem, IconButton, Tooltip } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import type { EvaluationItem } from '../types/blend.types';
import { useAutoSave } from '../../../shared/hooks/useAutoSave';
import { useBlendMutations } from '../hooks/useBlendMutations';

interface EvaluationRowProps {
  blendId: number;
  blendCode: string;
  sectionId: number;
  sectionTitle: string;
  item: EvaluationItem;
  onChange: (id: number, field: keyof EvaluationItem, value: string) => void;
  onDelete: (id: number) => void;
  readOnly?: boolean;
}

export const EvaluationRow: React.FC<EvaluationRowProps> = ({ blendId, blendCode, sectionId, sectionTitle, item, onChange, onDelete, readOnly = false }) => {
  const { updateEvaluationAsync } = useBlendMutations(blendId);
    
  useAutoSave({
      value: item,
      delay: 800,
      onSave: async (debounced) => {
        if (readOnly) return;

        await updateEvaluationAsync({
          evaluationId:debounced.evaluationId, 
          payload: {
          evaluationParameter: debounced.evaluationParameter,
          specification: debounced.specification,
          result: debounced.result,
          status: debounced.status,
          blendId: blendId,
          blendCode: blendCode,
          sectionId: sectionId,
          sectionTitle: sectionTitle,
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
          slotProps={{ htmlInput: { readOnly } }}
          onChange={(e) => onChange(item.evaluationId, 'evaluationParameter', e.target.value)}
        />
      </TableCell>
      <TableCell>
        <TextField
          size="small"
          fullWidth
          value={item.result ?? ''}
          slotProps={{ htmlInput: { readOnly } }}
          onChange={(e) => onChange(item.evaluationId, 'result', e.target.value)}
        />
      </TableCell>
      <TableCell>
        <TextField
          size="small"
          fullWidth
          value={item.specification ?? ''}
          slotProps={{ htmlInput: { readOnly } }}
          onChange={(e) => onChange(item.evaluationId, 'specification', e.target.value)}
        />
      </TableCell>
      <TableCell>
        <Select
          size="small"
          fullWidth
          value={item.status ?? 'Pending'}
          disabled={readOnly}
          onChange={(e) => onChange(item.evaluationId, 'status', e.target.value)}
        >
          <MenuItem value="Pending">Pending</MenuItem>
          <MenuItem value="Pass">Pass</MenuItem>
          <MenuItem value="Fail">Fail</MenuItem>
        </Select>
      </TableCell>
      {!readOnly && <TableCell align="center">
        <Tooltip title="Delete parameter">
          <IconButton
            color="error"
            size="small"
            onClick={() => onDelete(item.evaluationId)}
          >
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </TableCell>}
    </TableRow>
  );
};