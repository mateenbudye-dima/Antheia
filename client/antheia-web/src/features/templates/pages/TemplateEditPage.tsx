import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Container, Box, Button, Typography, Paper } from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { TemplateEditor } from '..';

export const TemplateEditPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const templateId = id ? parseInt(id, 10) : null;

  if (!templateId || isNaN(templateId)) {
    return (
      <Container maxWidth="md" sx={{ py: { xs: 1.5, sm: 2, md: 3 } }}>
        <Paper variant="outlined" sx={{ p: { xs: 2, sm: 3 } }}>
          <Typography variant="h6" color="error" gutterBottom>
            Invalid Template ID
          </Typography>
          <Button
            variant="outlined"
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate('/templates')}
          >
            Back to Templates
          </Button>
        </Paper>
      </Container>
    );
  }

  return (
    <Box className="template-edit-page">
      {/* 
        Responsive Padding:
        - xs: 8px horizontal, 12px vertical
        - sm: 16px horizontal, 16px vertical
        - md+: 24px horizontal, 24px vertical
      */}
      <Container
        maxWidth="md"
        sx={{
          px: { xs: 1, sm: 1, md: 3 },
          py: { xs: 1, sm: 1, md: 3 },
        }}
      >
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/templates')}
          sx={{ mb: { xs: 1, sm: 1 } }}
          size="small"
        >
          Back to Templates
        </Button>

        <TemplateEditor />
      </Container>
    </Box>
  );
};