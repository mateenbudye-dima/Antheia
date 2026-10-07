import React, { useState } from 'react';
import {
  Paper,
  Box,
  Button,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Divider,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import SendIcon from '@mui/icons-material/Send';
import ScienceIcon from '@mui/icons-material/Science';
import BlenderIcon from '@mui/icons-material/Blender';
import FactCheckIcon from '@mui/icons-material/FactCheck';

import { SectionType } from '../types/blend.types';
import { useBlendMutations } from '../hooks/useBlendMutations';
import type { CreateSectionPayload } from '../api/blendsApi';

interface EditorToolbarProps {
  blendId: number;
  blendCode: string;
  onSendForApproval: () => void;
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
    hasDividerAfter: true,
  },
  {
    type: SectionType.Evaluation,
    label: 'Evaluation Parameters',
    icon: <FactCheckIcon fontSize="small" />,
  },
];

export const EditorToolbar: React.FC<EditorToolbarProps> = ({
  blendId,
  blendCode,
  onSendForApproval,
}) => {
  const { addSection, isSaving } = useBlendMutations(blendId);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const isMenuOpen = Boolean(anchorEl);

  const handleOpenMenu = (e: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(e.currentTarget);
  };

  const handleCloseMenu = () => {
    setAnchorEl(null);
  };

  const handleAddSection = (sectionTypeId: SectionType) => {
    handleCloseMenu();
    const payload: CreateSectionPayload = {
      blendCode: blendCode || '',
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

  return (
    <Paper
      elevation={0}
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        py: 0.5,
        px: 1,
        mt: 1,
        borderRadius: 1,
        backgroundColor: 'action.hover',
      }}
    >
      <Box sx={{ display: 'flex', gap: 1 }}>
        <Button
          size="small"
          variant="outlined"
          startIcon={<AddIcon />}
          onClick={handleOpenMenu}
          disabled={isSaving}
        >
          {isSaving ? 'Adding...' : 'Add Section'}
        </Button>
        <Menu
          anchorEl={anchorEl}
          open={isMenuOpen}
          onClose={handleCloseMenu}
          transformOrigin={{ horizontal: 'left', vertical: 'top' }}
          anchorOrigin={{ horizontal: 'left', vertical: 'bottom' }}
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

      <Box sx={{ ml: 'auto' }}>
        <Button
          size="small"
          variant="contained"
          color="primary"
          endIcon={<SendIcon />}
          onClick={onSendForApproval}
        >
          Send for Approval
        </Button>
      </Box>
    </Paper>
  );
};