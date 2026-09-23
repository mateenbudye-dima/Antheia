import React, { useState } from 'react';
import { Paper, Grid, TextField, InputAdornment } from '@mui/material';
import type { PreparationMethod } from '../types/template.types';
import { useAutoSave } from '../../../shared/hooks/useAutoSave';
import { useTemplateMutations } from '../hooks/useTemplateMutations';

export const PrepMethodSection: React.FC<{templateId:number, prepData: PreparationMethod }> = ({ templateId, prepData }) => {
  const [prep, setPrep] = useState<PreparationMethod>(prepData);

  const {updatePrepMethodAsync} = useTemplateMutations(templateId);

  useAutoSave({
    value: prep,
    delay: 800,
    onSave: async (debouncedPrep) => {
      await updatePrepMethodAsync({
        prepId:debouncedPrep.preparationId, 
        payload:{
        additionSequence: debouncedPrep.additionSequence,
        mixingSpeed: debouncedPrep.mixingSpeed,
        mixingTime: debouncedPrep.mixingTime,
        temperature: debouncedPrep.temperature,
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