import React, { useState } from 'react';
import { Paper, Grid, TextField, Typography, Box, InputAdornment } from '@mui/material';
import { useAutoSave } from '../../../shared/hooks/useAutoSave';
import { useBlendMutations } from '../hooks/useBlendMutations';
import type { UpdateBlendHeaderDto } from '../api/blendsApi';
interface Props {
  blendId: number;
  initialTitle: string;
  initialTrialNumber?: string;
  initialObjective: string;
  initialDescription: string;
}

export const HeaderSection: React.FC<Props> = ({
  blendId,
  initialTitle,
  initialTrialNumber = '',
  initialObjective,
  initialDescription,
}) => {
  const [headerData, setHeaderData] = useState({
    code: initialTitle,
    trialNumber: initialTrialNumber,
    objective: initialObjective,
    description: initialDescription,
  });

  const { updateHeaderAsync } = useBlendMutations(blendId);

  useAutoSave({
    value: headerData,
    delay: 800,
    onSave: async (debounced) => {
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
            onKeyDown={handleNumberKeyDown}
            slotProps={{
              htmlInput: { min: 1 },
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
            onChange={(e) => handleChange('description', e.target.value)}
          />
        </Grid>
      </Grid>
    </Paper>
  );
};