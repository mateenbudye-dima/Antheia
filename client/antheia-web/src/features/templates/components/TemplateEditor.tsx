import React from 'react';
import { Container, Box } from '@mui/material';
import { TemplateHeaderSection } from './TemplateHeaderSection';
import { IngredientsSection } from './IngredientsSection';
import { PrepMethodSection } from './PrepMethodSection';
import { EvaluationSection } from './EvaluationSection';
import type { FullTemplateResponse } from '../types/template.types';

export interface TemplateEditorProps {
  data: FullTemplateResponse;
  selectedSectionId: string;
  viewMode: 'split' | 'all';
}

export const TemplateEditor: React.FC<TemplateEditorProps> = ({
  data,
  selectedSectionId,
  viewMode,
}) => {
  // Extract specific sections safely from FullTemplateResponse
  const ingredientsSection = data.sections?.find(
    (sec) => sec.sectionTypeId === 1
  );
  const prepSection = data.sections?.find(
    (sec) => sec.sectionTypeId === 2
  );
  const evaluationSection = data.sections?.find(
    (sec) => sec.sectionTypeId === 3
  );

  // Render Section 0: Header
  const renderHeader = () => (
    <TemplateHeaderSection
      templateId={data.templateId}
      initialTitle={data.title || ''}
      initialObjective={data.objective || ''}
      initialDescription={data.description || ''}
    />
  );

  // Render Section 1: Ingredients
  const renderIngredients = () =>
    ingredientsSection ? (
      <IngredientsSection
        sectionId={ingredientsSection.sectionId}
        initialIngredients={ingredientsSection.ingredients || []}
      />
    ) : null;

  // Render Section 2: Preparation Method
  const renderPrepMethod = () =>
    prepSection?.preparationMethod ? (
      <PrepMethodSection prepData={prepSection.preparationMethod} />
    ) : null;

  // Render Section 3: Evaluation Parameters
  const renderEvaluation = () =>
    evaluationSection ? (
      <EvaluationSection
        sectionId={evaluationSection.sectionId}
        initialEvaluations={evaluationSection.evaluations || []}
      />
    ) : null;

  return (
    <Container maxWidth="lg" sx={{ py: 2 }}>
      {viewMode === 'split' ? (
        /* 1. SPLIT VIEW: Show only the active selected section */
        <Box>
          {selectedSectionId === 'header' && renderHeader()}
          {selectedSectionId === 'section_1' && renderIngredients()}
          {selectedSectionId === 'section_2' && renderPrepMethod()}
          {selectedSectionId === 'section_3' && renderEvaluation()}
        </Box>
      ) : (
        /* 2. COMPLETE VIEW: Show all sections stacked sequentially */
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
          {renderHeader()}
          {renderIngredients()}
          {renderPrepMethod()}
          {renderEvaluation()}
        </Box>
      )}
    </Container>
  );
};