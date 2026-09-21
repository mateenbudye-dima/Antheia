import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Container, CircularProgress, Alert, Box } from '@mui/material';
import { TemplateHeaderSection } from './TemplateHeaderSection';
import { IngredientsSection } from './IngredientsSection';
import { PrepMethodSection } from './PrepMethodSection';
import { EvaluationSection } from './EvaluationSection';
import type { FullTemplateResponse } from '../types/template.types';
import { templatesApi } from '../api/templatesApi';

export const TemplateEditor: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [data, setData] = useState<FullTemplateResponse | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchTemplate = async () => {
      try {
        if (!id) return;
        const result = await templatesApi.getFullTemplate(Number(id));
        setData(result);
      } catch (err) {
        console.error('Error loading template:', err);
        setError('Failed to load template data.');
      } finally {
        setLoading(false);
      }
    };

    if (id) fetchTemplate();
  }, [id]);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error || !data) {
    return (
      <Container maxWidth="md" sx={{ mt: 4 }}>
        <Alert severity="error">{error || 'Template not found'}</Alert>
      </Container>
    );
  }

  const ingredientsSection = data.sections?.find((s) => s.sectionTypeId === 1);
  const prepSection = data.sections?.find((s) => s.sectionTypeId === 2);
  const evaluationSection = data.sections?.find((s) => s.sectionTypeId === 3);

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {/* 1. Editable Header Section */}
      <TemplateHeaderSection
        templateId={data.templateId}
        initialTitle={data.title || ''}
        initialObjective={data.objective || ''}
        initialDescription={data.description || ''}
      />

      {/* 2. Section 1: Ingredients */}
      {ingredientsSection && (
        <IngredientsSection
          sectionId={ingredientsSection.sectionId}
          initialIngredients={ingredientsSection.ingredients || []}
        />
      )}

      {/* 3. Section 2: Preparation Method */}
      {prepSection?.preparationMethod && (
        <PrepMethodSection prepData={prepSection.preparationMethod} />
      )}

      {/* 4. Section 3: Evaluation Parameters */}
      {evaluationSection && (
        <EvaluationSection
          sectionId={evaluationSection.sectionId}
          initialEvaluations={evaluationSection.evaluations || []}
        />
      )}
    </Container>
  );
};