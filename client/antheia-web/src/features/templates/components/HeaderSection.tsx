import React, { useState } from 'react';
import { Paper, Grid, TextField, Typography, Box } from '@mui/material';
import { useAutoSave } from '../../../shared/hooks/useAutoSave';
import { StatusBadge } from '../../../shared/components/StatusBadge';
import { useTemplateMutations } from '../hooks/useTemplateMutations';

interface Props {
  templateId: number;
  initialTitle: string;
  initialObjective: string;
  initialDescription: string;
}

export const HeaderSection: React.FC<Props> = ({
  templateId,
  initialTitle,
  initialObjective,
  initialDescription,
}) => {
  const [headerData, setHeaderData] = useState({
    title: initialTitle,
    objective: initialObjective,
    description: initialDescription,
  });

  const {updateHeaderAsync} = useTemplateMutations(templateId);

  const { status } = useAutoSave({
    value: headerData,
    delay: 800,
    onSave: async (debounced) => {
      await updateHeaderAsync(debounced);
    },
  });

  const handleChange = (field: keyof typeof headerData, value: string) => {
    setHeaderData((prev) => ({ ...prev, [field]: value }));
  };

  return (
    <Paper sx={{ p: 3, mb: 3 }} variant="outlined">
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h5" component="h1" sx={{ fontWeight: 'bold' }}>
          Template Header Details
        </Typography>
        <StatusBadge status={status} />
      </Box>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12 }}>
          <TextField
            label="Template Title"
            size="small"
            fullWidth
            value={headerData.title ?? ''}
            onChange={(e) => handleChange('title', e.target.value)}
          />
        </Grid>

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
