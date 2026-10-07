import React from 'react';
import {
  Container,
  Box,
  IconButton,
  Typography,
  Tooltip,
} from '@mui/material';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';

import { HeaderSection } from './HeaderSection';
import { IngredientsSection } from './IngredientsSection';
import { PrepMethodSection } from './PrepMethodSection';
import { EvaluationSection } from './EvaluationSection';
import { SectionType, type FullBlendResponse, type Section } from '../types/blend.types';
import { useBlendMutations } from '../hooks/useBlendMutations';
import { useConfirm } from '../../../shared/context/DialogContext';

export interface BlendEditorProps {
  data: FullBlendResponse;
  selectedSectionId: string;
  viewMode: 'split' | 'all';
  onSectionDeleted?: () => void;
}

export const BlendEditor: React.FC<BlendEditorProps> = ({
  data,
  selectedSectionId,
  viewMode,
  onSectionDeleted,
}) => {
  const { deleteSection } = useBlendMutations(data.blendId);
  const confirm = useConfirm();

  const handleDeleteClick = (section: Section) => {
    confirm({
      title: 'Delete Section?',
      message: `Are you sure you want to delete "${
        section.sectionTitle || 'this section'
      }"? All contained data will be removed.`,
      confirmText: 'Delete',
      confirmColor: 'error',
      onConfirm: async () => {
        await new Promise<void>((resolve, reject) => {
          deleteSection(section.sectionId, {
            onSuccess: () => {
              if (String(section.sectionId) === selectedSectionId && onSectionDeleted) {
                onSectionDeleted();
              }
              resolve();
            },
            onError: (err) => reject(err),
          });
        });
      },
    });
  };

  const renderSectionWrapper = (section: Section, children: React.ReactNode) => {
    return (
      <Box key={section.sectionId} sx={{ mb: 4, position: 'relative' }}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            mb: 2,
            pb: 1,
            px: 1,
            backgroundColor: 'action.hover',
            borderRadius: 1,
            borderBottom: '2px solid',
            borderColor: 'primary.main',
          }}
        >
          <Typography variant="h6" color="text.primary" sx={{ fontWeight: 'medium' }}>
            {section.sectionTitle ||
              (section.sectionTypeId === SectionType.Ingredients
                ? 'Ingredients'
                : section.sectionTypeId === SectionType.PreparationMethod
                ? 'Preparation Method'
                : 'Evaluation Parameters')}
          </Typography>
          <Tooltip title="Delete Section">
            <IconButton
              color="error"
              size="small"
              onClick={() => handleDeleteClick(section)}
            >
              <DeleteOutlineIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>
        {children}
      </Box>
    );
  };

  const renderSectionNode = (section: Section) => {
    switch (section.sectionTypeId) {
      case SectionType.Ingredients:
        return renderSectionWrapper(
          section,
          <IngredientsSection
            key={section.sectionId}
            blendId={data.blendId}
            blendCode={data.code || ''}
            sectionId={section.sectionId}
            sectionTitle={section.sectionTitle || ''}
            initialIngredients={section.ingredients || []}
          />
        );
      case SectionType.PreparationMethod:
        return section.preparationMethod ? renderSectionWrapper(
          section,
          <PrepMethodSection
            key={section.sectionId}
            blendId={data.blendId}
            sectionId={section.sectionId}
            sectionTitle={section.sectionTitle || ''}
            blendCode={data.code || ''}
            prepData={section.preparationMethod}
          />
        ) : null;
      case SectionType.Evaluation:
        return renderSectionWrapper(
          section,
          <EvaluationSection
            key={section.sectionId}
            blendId={data.blendId}
            sectionId={section.sectionId}
            sectionTitle={section.sectionTitle || ''}
            blendCode={data.code || ''}
            initialEvaluations={section.evaluations || []}
          />
        );
      default:
        return null;
    }
  };

  const renderHeader = () => (
    <HeaderSection
      blendId={data.blendId}
      initialCode={data.code || ''}
      initialTrialNumber={data.trialNumber || ''}
      initialObjective={data.objective || ''}
      initialDescription={data.description || ''}
    />
  );

  return (
    <Container maxWidth="lg" sx={{ py: 2 }}>
      {viewMode === 'split' ? (
        <Box>
          {selectedSectionId === 'header' && renderHeader()}
          {data.sections
            ?.filter((sec: Section) => String(sec.sectionId) === selectedSectionId)
            .map((sec) => renderSectionNode(sec))}
        </Box>
      ) : (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
          {renderHeader()}
          {data.sections?.map((sec: Section) => renderSectionNode(sec))}
        </Box>
      )}
    </Container>
  );
};