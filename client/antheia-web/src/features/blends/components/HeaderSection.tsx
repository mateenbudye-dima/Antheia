import React, { useState } from 'react';
import { Paper, Grid, TextField, Typography, Box, InputAdornment } from '@mui/material';
import { useAutoSave } from '../../../shared/hooks/useAutoSave';
import { useBlendMutations } from '../hooks/useBlendMutations';
import type { UpdateBlendHeaderDto } from '../api/blendsApi';
interface Props {
  blendId: number;
  initialCode: string;
  initialTrialNumber?: string;
  initialObjective: string;
  initialDescription: string;
  readOnly?: boolean;
}

export const HeaderSection: React.FC<Props> = ({
  blendId,
  initialCode,
  initialTrialNumber = '',
  initialObjective,
  initialDescription,
  readOnly = false,
}) => {
  const [headerData, setHeaderData] = useState({
    code: initialCode,
    trialNumber: initialTrialNumber,
    objective: initialObjective,
    description: initialDescription,
  });

  const { updateHeaderAsync } = useBlendMutations(blendId);

  useAutoSave({
    value: headerData,
    delay: 800,
    onSave: async (debounced) => {
      if (readOnly) return;

      // Format payload ensuring trialNumber is sent as string or undefined
      const payload: UpdateBlendHeaderDto = {
        code: debounced.code,
        objective: debounced.objective,
        description: debounced.description,
        trialNumber: debounced.trialNumber ? String(debounced.trialNumber) : undefined,
      };

      await updateHeaderAsync(payload);
    },
  });

  const handleChange = (field: keyof typeof headerData, value: string) => {
    setHeaderData((prev) => ({ ...prev, [field]: value }));
  };

  // Prevent non-numeric key entries ('e', 'E', '+', '-') in trial number input
  const handleNumberKeyDown = (e: React.KeyboardEvent) => {
    if (['e', 'E', '+', '-'].includes(e.key)) {
      e.preventDefault();
    }
  };

  return (
    <Paper sx={{ p: 3, mb: 3 }} variant="outlined">
      <Box
        sx={{
          display: 'flex',
          justify: 'space-between',
          alignItems: 'center',
          mb: 2,
        }}
      >
        <Typography variant="h5" component="h1" sx={{ fontWeight: 'bold' }}>
          Blend Header Details
        </Typography>
      </Box>

      <Grid container spacing={2}>
        {/* Blend Code */}
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            label="Blend Code"
            size="small"
            fullWidth
            value={headerData.code ?? ''}
            slotProps={{ htmlInput: { readOnly } }}
            onChange={(e) => handleChange('code', e.target.value)}
          />
        </Grid>

        {/* Trial Number */}
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            label="Trial Number"
            size="small"
            type="number"
            fullWidth
            value={headerData.trialNumber ?? ''}
            slotProps={{
              htmlInput: { min: 1, readOnly },
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <Typography variant="body2" color="text.secondary">
                      T-
                    </Typography>
                  </InputAdornment>
                ),
              },
            }}
            onKeyDown={handleNumberKeyDown}
            onChange={(e) => handleChange('trialNumber', e.target.value)}
          />
        </Grid>

        {/* Objective */}
        <Grid size={{ xs: 12 }}>
          <TextField
            label="Objective"
            size="small"
            fullWidth
            multiline
            rows={2}
            value={headerData.objective ?? ''}
            slotProps={{ htmlInput: { readOnly } }}
            onChange={(e) => handleChange('objective', e.target.value)}
          />
        </Grid>

        {/* Description */}
        <Grid size={{ xs: 12 }}>
          <TextField
            label="Description"
            size="small"
            fullWidth
            multiline
            rows={2}
            value={headerData.description ?? ''}
            slotProps={{ htmlInput: { readOnly } }}
            onChange={(e) => handleChange('description', e.target.value)}
          />
        </Grid>
      </Grid>
    </Paper>
  );
};