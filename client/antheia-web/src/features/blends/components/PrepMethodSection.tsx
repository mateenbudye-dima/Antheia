import React, { useState } from 'react';
import { Paper, Grid, TextField, InputAdornment } from '@mui/material';
import type { PreparationMethod } from '../types/blend.types';
import { useAutoSave } from '../../../shared/hooks/useAutoSave';
import { useBlendMutations } from '../hooks/useBlendMutations';

interface Props {
  blendId: number;
  sectionId: number;
  sectionTitle: string;
  blendCode: string;
  prepData: PreparationMethod;
  readOnly?: boolean;
}

export const PrepMethodSection: React.FC<Props> = ({ blendId, sectionId, sectionTitle, blendCode, prepData, readOnly = false }) => {
  const [prep, setPrep] = useState<PreparationMethod>(prepData);

  const {updatePrepMethodAsync} = useBlendMutations(blendId );

  useAutoSave({
    value: prep,
    delay: 800,
    onSave: async (debouncedPrep) => {
      if (readOnly) return;

      await updatePrepMethodAsync({
        prepId:debouncedPrep.preparationId, 
        payload:{
        additionSequence: debouncedPrep.additionSequence,
        mixingSpeed: debouncedPrep.mixingSpeed,
        mixingTime: debouncedPrep.mixingTime,
        temperature: debouncedPrep.temperature,
        sectionId: sectionId,
        sectionTitle: sectionTitle,
        blendId: blendId,
        blendCode: blendCode,
        }
      });
    },
  });

  const handleChange = (field: keyof PreparationMethod, value: string) => {
    setPrep((prev) => ({ ...prev, [field]: value }));
  };

  return (
    <Paper sx={{ p: 2, mt: 3 }} variant="outlined">
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            label="Addition Sequence"
            size="small"
            fullWidth
            value={prep.additionSequence ?? ''}
            slotProps={{ htmlInput: { readOnly } }}
            onChange={(e) => handleChange('additionSequence', e.target.value)}
          />
        </Grid>

        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            label="Mixing Speed"
            size="small"
            fullWidth
            value={prep.mixingSpeed ?? ''}
            onChange={(e) => handleChange('mixingSpeed', e.target.value)}
            slotProps={{
              htmlInput: { readOnly },
              input: {
                endAdornment: <InputAdornment position="end">RPM</InputAdornment>,
              },
            }}
          />
        </Grid>

        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            label="Mixing Time"
            size="small"
            fullWidth
            value={prep.mixingTime ?? ''}
            onChange={(e) => handleChange('mixingTime', e.target.value)}
            slotProps={{
              htmlInput: { readOnly },
              input: {
                endAdornment: <InputAdornment position="end">Mins</InputAdornment>,
              },
            }}
          />
        </Grid>

        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            label="Temperature"
            size="small"
            fullWidth
            value={prep.temperature ?? ''}
            onChange={(e) => handleChange('temperature', e.target.value)}
            slotProps={{
              htmlInput: { readOnly },
              input: {
                endAdornment: <InputAdornment position="end">°C</InputAdornment>,
              },
            }}
          />
        </Grid>
      </Grid>
    </Paper>
  );
};