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
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  IconButton,
  Typography,
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
import type { FullTemplateResponse, Section } from '../types/template.types';
import { useTemplateMutations } from '../hooks/useTemplateMutations';
import type { CreateSectionPayload } from '../api/templatesApi';

export interface TemplateEditorProps {
  data: FullTemplateResponse;
  selectedSectionId: string; // E.g., 'header' or specific database sectionId like '102'
  viewMode: 'split' | 'all';
  onSectionDeleted?: () => void; // Optional callback to reset selection

}

export const TemplateEditor: React.FC<TemplateEditorProps> = ({
  data,
  selectedSectionId,
  viewMode,
  onSectionDeleted,
}) => {
  const { addSection, deleteSection, isSaving } = useTemplateMutations(data.templateId);

  // Dropdown Menu State
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const isMenuOpen = Boolean(anchorEl);

  const handleOpenMenu = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleCloseMenu = () => {
    setAnchorEl(null);
  };

  // Delete Confirmation Modal State
  const [sectionToDelete, setSectionToDelete] = useState<Section | null>(null);

  const handleAddSection = (sectionTypeId: 1 | 2 | 3) => {
    handleCloseMenu();
    const payload: CreateSectionPayload = {
      sectionTypeId,
      sectionTitle:
        sectionTypeId === 1
          ? 'Ingredients'
          : sectionTypeId === 2
          ? 'Preparation Method'
          : 'Evaluation Parameters',
    };
    addSection(payload);
  };

  const handleConfirmDelete = () => {
    if (sectionToDelete) {
      deleteSection(sectionToDelete.sectionId, {
        onSuccess: () => {
          setSectionToDelete(null);
          // If deleted section was currently active, notify parent to select 'header'
          if (String(sectionToDelete.sectionId) === selectedSectionId && onSectionDeleted) {
            onSectionDeleted();
          }
        },
      });
    }
  };
  // Section Wrapper with Title & Delete Icon
  const renderSectionWrapper = (section: Section, children: React.ReactNode) => {
    return (
      <Box key={section.sectionId} sx={{ mb: 4, position: 'relative' }}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            justify: 'space-between',
            mb: 1,
            pb: 0.5,
            borderBottom: '1px solid',
            borderColor: 'divider',
          }}
        >
          <Typography sx={{ variant: "h6", color:"text.primary" }} >
            {section.sectionTitle ||
              (section.sectionTypeId === 1
                ? 'Ingredients'
                : section.sectionTypeId === 2
                ? 'Preparation Method'
                : 'Evaluation Parameters')}
          </Typography>
          <IconButton
            color="error"
            size="small"
            onClick={() => setSectionToDelete(section)}
            title="Delete Section"
          >
            <DeleteOutlineIcon fontSize="small" />
          </IconButton>
        </Box>
        {children}
      </Box>
    );
  };

  // Helper to render a individual section based on its sectionTypeId
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

  // Render Header Component
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
      {/* Add Section Controls */}
      <Box sx={{ mb: 2, pt: 2, borderTop: 1, borderColor: 'divider' }}>
        <Button
          variant="outlined"
          startIcon={<AddIcon />}
          onClick={handleOpenMenu}
          disabled={isSaving}
        >
          {isSaving ? 'Adding Section...' : 'Add Section'}
        </Button>

        <Menu
          anchorEl={anchorEl}
          open={isMenuOpen}
          onClose={handleCloseMenu}
          transformOrigin={{ horizontal: 'left', vertical: 'top' }}
          anchorOrigin={{ horizontal: 'left', vertical: 'bottom' }}
        >
          <MenuItem onClick={() => handleAddSection(1)}>
            <ListItemIcon>
              <FastfoodIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText primary="Ingredients Section" />
          </MenuItem>

          <MenuItem onClick={() => handleAddSection(2)}>
            <ListItemIcon>
              <BuildIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText primary="Preparation Method" />
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

      {viewMode === 'split' ? (
        /* 1. SPLIT VIEW: Render selected item dynamically */
        <Box>
          {selectedSectionId === 'header' && renderHeader()}
          {data.sections
            ?.filter((sec) => String(sec.sectionId) === selectedSectionId)
            .map((sec) => renderSectionNode(sec))}
        </Box>
      ) : (
        /* 2. COMPLETE VIEW: Stack header and all array items sequentially */
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
          {renderHeader()}
          {data.sections?.map((sec) => renderSectionNode(sec))}
        </Box>
      )}

      {/* Delete Confirmation Modal */}
      <Dialog open={Boolean(sectionToDelete)} onClose={() => setSectionToDelete(null)}>
        <DialogTitle>Delete Section?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete this section? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSectionToDelete(null)}>Cancel</Button>
          <Button color="error" variant="contained" onClick={handleConfirmDelete}>
            Delete
          </Button>
        </DialogActions>
      </Dialog>

    </Container>
  );
};