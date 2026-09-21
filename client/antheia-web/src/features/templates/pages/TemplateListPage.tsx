import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { isAxiosError } from 'axios';
import {
  Box,
  Container,
  Typography,
  Button,
  Card,
  CardContent,
  CardActions,
  Chip,
  Grid,
  Alert,
  Skeleton,
  Paper,
  Stack,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import ArrowForwardIcon from '@mui/icons-material/ArrowForward';
import apiClient from '../../../shared/api/apiClient';

interface TemplateItem {
  templateId: number;
  title: string;
  objective: string | null;
  updatedDate: string;
  isPublished: boolean | null;
}

export const TemplateListPage: React.FC = () => {
  const [templates, setTemplates] = useState<TemplateItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [isCreating, setIsCreating] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const navigate = useNavigate();

  useEffect(() => {
    const fetchTemplates = async () => {
      try {
        const response = await apiClient.get<TemplateItem[]>('/templates');
        setTemplates(response.data);
      } catch (err: unknown) {
        if (isAxiosError(err)) {
          setError(err.response?.data?.message || 'Failed to load templates.');
        } else if (err instanceof Error) {
          setError(err.message);
        } else {
          setError('An unexpected error occurred.');
        }
      } finally {
        setLoading(false);
      }
    };

    fetchTemplates();
  }, []);

  const handleCreateNewTemplate = async () => {
    setIsCreating(true);
    try {
      const response = await apiClient.post<{ templateId: number }>('/templates/draft');
      navigate(`/templates/${response.data.templateId}/edit`);
    } catch (err: unknown) {
      if (isAxiosError(err)) {
        alert(err.response?.data?.message || 'Could not create template draft.');
      } else if (err instanceof Error) {
        alert(err.message);
      } else {
        alert('Failed to create new draft');
      }
      setIsCreating(false);
    }
  };

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Grid container spacing={3}>
          {[1, 2, 3].map((key) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={key}>
              <Skeleton variant="rounded" height={180} />
            </Grid>
          ))}
        </Grid>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {/* Header Section */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 4,
          flexWrap: 'wrap',
          gap: 2,
        }}
      >
        <Box>
          <Typography variant="h4" component="h1" sx={{ fontWeight: 'bold' }} gutterBottom>
            Formulation Templates
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Manage and edit your platform product template specifications.
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={handleCreateNewTemplate}
          disabled={isCreating}
          size="large"
        >
          {isCreating ? 'Initializing Draft...' : 'Create New Template'}
        </Button>
      </Box>

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Templates Grid / Empty State */}
      {templates.length === 0 ? (
        <Paper
          elevation={0}
          sx={{
            p: 6,
            textAlign: 'center',
            backgroundColor: 'action.hover',
            borderRadius: 2,
          }}
        >
          <Typography color="text.secondary">
            No templates found. Click <strong>"+ Create New Template"</strong> to start a new draft.
          </Typography>
        </Paper>
      ) : (
        <Grid container spacing={3}>
          {templates.map((template) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={template.templateId}>
              <Card
                variant="outlined"
                sx={{
                  height: '100%',
                  display: 'flex',
                  flexDirection: 'column',
                  justifyContent: 'space-between',
                  transition: 'transform 0.2s, box-shadow 0.2s',
                  '&:hover': {
                    boxShadow: 4,
                  },
                }}
              >
                <CardContent>
                  <Stack
                    direction="row"
                    spacing={1}
                    sx={{
                      justifyContent: 'space-between',
                      alignItems: 'flex-start',
                      mb: 1.5,
                    }}
                  >
                    <Typography variant="h6" component="h2" sx={{ fontSize: '1.1rem', fontWeight: '600' }}>
                      {template.title || 'Untitled Template'}
                    </Typography>
                    <Chip
                      label={template.isPublished ? 'Published' : 'Draft'}
                      color={template.isPublished ? 'success' : 'warning'}
                      size="small"
                      variant="outlined"
                      sx={{ fontWeight: 'bold' }}
                    />
                  </Stack>

                  <Typography
                    variant="body2"
                    color="text.secondary"
                    sx={{
                      minHeight: 40,
                      display: '-webkit-box',
                      WebkitLineClamp: 3,
                      WebkitBoxOrient: 'vertical',
                      overflow: 'hidden',
                    }}
                  >
                    {template.objective || 'No objective provided.'}
                  </Typography>
                </CardContent>

                <CardActions
                  sx={{
                    justifyContent: 'space-between',
                    px: 2,
                    pb: 2,
                    pt: 0,
                    borderTop: 1,
                    borderColor: 'divider',
                  }}
                >
                  <Typography variant="caption" color="text.secondary" sx={{ flexGrow: 1 }}>
                    Updated: {new Date(template.updatedDate).toLocaleDateString()}
                  </Typography>
                  <Button
                    size="small"
                    endIcon={<ArrowForwardIcon />}
                    onClick={() => navigate(`/templates/${template.templateId}/edit`)}
                  >
                    Edit Template
                  </Button>
                </CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}
    </Container>
  );
};