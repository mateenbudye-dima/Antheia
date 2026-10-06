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
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined';
import ScienceIcon from '@mui/icons-material/Science'; // Ingredient
import BlenderIcon from '@mui/icons-material/Blender'; // Preparation Method
import FactCheckIcon from '@mui/icons-material/FactCheck'; // Evaluation Params


import { HeaderSection } from './HeaderSection';
import { IngredientsSection } from './IngredientsSection';
import { PrepMethodSection } from './PrepMethodSection';
import { EvaluationSection } from './EvaluationSection';
import { SectionType, type FullBlendResponse, type Section } from '../types/blend.types';
import { useBlendMutations } from '../hooks/useBlendMutations';
import type { CreateSectionPayload } from '../api/blendsApi';
import { useConfirm } from '../../../shared/context/DialogContext'; // 👈 Global hook

export interface BlendEditorProps {
  data: FullBlendResponse;
  selectedSectionId: string;
  viewMode: 'split' | 'all';
  onSectionDeleted?: () => void;
}
interface MenuOption {
  type: SectionType;
  label: string;
  icon: React.ReactNode;
  hasDividerAfter?: boolean;
}

const SECTION_OPTIONS: MenuOption[] = [
  {
    type: SectionType.Ingredients,
    label: 'Ingredients Section',
    icon: <ScienceIcon fontSize="small" />,
  },
  {
    type: SectionType.PreparationMethod,
    label: 'Preparation Method',
    icon: <BlenderIcon fontSize="small" />,
    hasDividerAfter: true, // Renders the divider after this item
  },
  {
    type: SectionType.Evaluation,
    label: 'Evaluation Parameters',
    icon: <FactCheckIcon fontSize="small" />,
  },
];

export const BlendEditor: React.FC<BlendEditorProps> = ({
  data,
  selectedSectionId,
  viewMode,
  onSectionDeleted,
}) => {
  const { addSection, deleteSection, isSaving } = useBlendMutations(data.blendId);
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

  //const hasPrepMethod = data.sections?.some((sec) => sec.sectionTypeId === SectionType.PreparationMethod);

  const handleAddSection = (sectionTypeId: SectionType) => {
    handleCloseMenu();
    const payload: CreateSectionPayload = {
      blendCode: data.code || '',
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
            blendId={data.blendId}
            blendCode={data.code || ''}
            sectionId={section.sectionId}
            sectionTitle={section.sectionTitle || ''}
            initialIngredients={section.ingredients || []}
          />
        );
      case 2:
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
      case 3:
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
          {SECTION_OPTIONS.map((option) => (
            <React.Fragment key={option.type}>
              <MenuItem onClick={() => handleAddSection(option.type)}>
                <ListItemIcon>{option.icon}</ListItemIcon>
                <ListItemText primary={option.label} />
              </MenuItem>

              {option.hasDividerAfter && <Divider />}
            </React.Fragment>
          ))}
        </Menu>
      </Box>
    </Container>
  );
};