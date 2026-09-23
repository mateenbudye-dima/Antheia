import React, { useState } from 'react';
import {
  Container,
  Box,
  Button,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Divider,
  IconButton,
  Typography,
  Tooltip,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import FastfoodIcon from '@mui/icons-material/Fastfood';
import BuildIcon from '@mui/icons-material/Build';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import AssessmentIcon from '@mui/icons-material/Assessment';

import { HeaderSection } from './HeaderSection';
import { IngredientsSection } from './IngredientsSection';
import { PrepMethodSection } from './PrepMethodSection';
import { EvaluationSection } from './EvaluationSection';
import { SectionType, type FullTemplateResponse, type Section } from '../types/template.types';
import { useTemplateMutations } from '../hooks/useTemplateMutations';
import type { CreateSectionPayload } from '../api/templatesApi';
import { useConfirm } from '../../../shared/context/DialogContext'; // 👈 Global hook

export interface TemplateEditorProps {
  data: FullTemplateResponse;
  selectedSectionId: string;
  viewMode: 'split' | 'all';
  onSectionDeleted?: () => void;
}

export const TemplateEditor: React.FC<TemplateEditorProps> = ({
  data,
  selectedSectionId,
  viewMode,
  onSectionDeleted,
}) => {
  const { addSection, deleteSection, isSaving } = useTemplateMutations(data.templateId);
  const confirm = useConfirm(); // 👈 Invoke confirmation dialog hook

  // Dropdown Menu State
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const isMenuOpen = Boolean(anchorEl);

  const handleOpenMenu = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleCloseMenu = () => {
    setAnchorEl(null);
  };

  const hasPrepMethod = data.sections?.some((sec) => sec.sectionTypeId === SectionType.PreparationMethod);

  const handleAddSection = (sectionTypeId: SectionType) => {
    handleCloseMenu();
    const payload: CreateSectionPayload = {
      sectionTypeId,
      sectionTitle:
        sectionTypeId === SectionType.Ingredients
          ? 'Ingredients'
          : sectionTypeId === SectionType.PreparationMethod
          ? 'Preparation Method'
          : 'Evaluation Parameters',
    };
    addSection(payload);
  };

  // 💥 DELETION TRIGGERED IMPERATIVELY
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

  // Section Wrapper with Title & Delete Action
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
      case 1:
        return renderSectionWrapper(
          section,
          <IngredientsSection
            key={section.sectionId}
            templateId={data.templateId}
            sectionId={section.sectionId}
            initialIngredients={section.ingredients || []}
          />
        );
      case 2:
        return section.preparationMethod ? renderSectionWrapper(
          section,
          <PrepMethodSection
            key={section.sectionId}
            templateId={data.templateId}
            prepData={section.preparationMethod}
          />
        ) : null;
      case 3:
        return renderSectionWrapper(
          section,
          <EvaluationSection
            key={section.sectionId}
            templateId={data.templateId}
            sectionId={section.sectionId}
            initialEvaluations={section.evaluations || []}
          />
        );
      default:
        return null;
    }
  };

  const renderHeader = () => (
    <HeaderSection
      templateId={data.templateId}
      initialTitle={data.title || ''}
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
            ?.filter((sec) => String(sec.sectionId) === selectedSectionId)
            .map((sec) => renderSectionNode(sec))}
        </Box>
      ) : (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
          {renderHeader()}
          {data.sections?.map((sec) => renderSectionNode(sec))}
        </Box>
      )}

      {/* Add Section Menu */}
      <Box sx={{ mt: 4, pt: 2, borderTop: 1, borderColor: 'divider', display: 'flex', justifyContent: 'center' }}>
        <Button
          variant="outlined"
          startIcon={<AddIcon />}
          onClick={handleOpenMenu}
          disabled={isSaving}
          size="large"
        >
          {isSaving ? 'Adding Section...' : 'Add Section'}
        </Button>

        <Menu
          anchorEl={anchorEl}
          open={isMenuOpen}
          onClose={handleCloseMenu}
          transformOrigin={{ horizontal: 'center', vertical: 'top' }}
          anchorOrigin={{ horizontal: 'center', vertical: 'bottom' }}
        >
          <MenuItem onClick={() => handleAddSection(1)}>
            <ListItemIcon>
              <FastfoodIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText primary="Ingredients Section" />
          </MenuItem>

          <MenuItem onClick={() => handleAddSection(2)} disabled={hasPrepMethod}>
            <ListItemIcon>
              <BuildIcon fontSize="small" color={hasPrepMethod ? 'disabled' : 'inherit'} />
            </ListItemIcon>
            <ListItemText
              primary="Preparation Method"
              secondary={hasPrepMethod ? 'Already added' : undefined}
            />
          </MenuItem>

          <Divider />

          <MenuItem onClick={() => handleAddSection(3)}>
            <ListItemIcon>
              <AssessmentIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText primary="Evaluation Parameters" />
          </MenuItem>
        </Menu>
      </Box>
    </Container>
  );
};