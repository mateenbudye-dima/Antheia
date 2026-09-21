import React, { useState } from 'react';
import { Paper, Grid, TextField, Typography, Box, InputAdornment } from '@mui/material';
import { templatesApi } from '../api/templatesApi';
import type { PreparationMethod } from '../types/template.types';
import { useAutoSave } from '../../../shared/hooks/useAutoSave';
import { StatusBadge } from '../../../shared/components/StatusBadge';

export const PrepMethodSection: React.FC<{ prepData: PreparationMethod }> = ({ prepData }) => {
  const [prep, setPrep] = useState<PreparationMethod>(prepData);

  const { status } = useAutoSave({
    value: prep,
    delay: 800,
    onSave: async (debouncedPrep) => {
      await templatesApi.updatePrepMethod(debouncedPrep.preparationId, {
        additionSequence: debouncedPrep.additionSequence,
        mixingSpeed: debouncedPrep.mixingSpeed,
        mixingTime: debouncedPrep.mixingTime,
        temperature: debouncedPrep.temperature,
      });
    },
  });

  const handleChange = (field: keyof PreparationMethod, value: string) => {
    setPrep((prev) => ({ ...prev, [field]: value }));
  };

  return (
    <Paper sx={{ p: 2, mt: 3 }} variant="outlined">
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h6">2. Preparation Method</Typography>
        <StatusBadge status={status} />
      </Box>

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