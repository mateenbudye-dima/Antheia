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
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import ScienceIcon from '@mui/icons-material/Science';
import BlenderIcon from '@mui/icons-material/Blender';
import FactCheckIcon from '@mui/icons-material/FactCheck';
import RateReviewIcon from '@mui/icons-material/RateReview';
import VerifiedIcon from '@mui/icons-material/Verified';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutlineOutlined';
import AssignmentTurnedInIcon from '@mui/icons-material/AssignmentTurnedIn';

import { SectionType } from '../types/blend.types';
import { useBlendMutations } from '../hooks/useBlendMutations';
import { useBlendWorkflowMutations } from '../hooks/useBlendWorkflowMutations';
import type { CreateSectionPayload } from '../api/blendsApi';
import { CommentDialog } from './CommentDialog';

export type ApprovalActionType =
  | 'SEND_FOR_REVIEW'
  | 'SEND_FOR_APPROVE'
  | 'SELF_REVIEW'
  | 'SELF_APPROVE';

interface EditorToolbarProps {
  blendId: number;
  blendCode: string;
  onActionSuccess?: (message: string) => void;
}

interface SectionMenuOption {
  type: SectionType;
  label: string;
  icon: React.ReactNode;
  hasDividerAfter?: boolean;
}

interface ApprovalOption {
  type: ApprovalActionType;
  label: string;
  icon: React.ReactNode;
  hasDividerAfter?: boolean;
}

const SECTION_OPTIONS: SectionMenuOption[] = [
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

const APPROVAL_OPTIONS: ApprovalOption[] = [
  {
    type: 'SEND_FOR_REVIEW',
    label: 'Send for Review',
    icon: <RateReviewIcon fontSize="small" />,
  },
  {
    type: 'SEND_FOR_APPROVE',
    label: 'Send for Approve',
    icon: <VerifiedIcon fontSize="small" />,
    hasDividerAfter: true,
  },
  {
    type: 'SELF_REVIEW',
    label: 'Self Review',
    icon: <CheckCircleOutlineIcon fontSize="small" />,
  },
  {
    type: 'SELF_APPROVE',
    label: 'Self Approve',
    icon: <AssignmentTurnedInIcon fontSize="small" />,
  },
];

export const EditorToolbar: React.FC<EditorToolbarProps> = ({
  blendId,
  blendCode,
  onActionSuccess,
}) => {
  const { addSection, isSaving } = useBlendMutations(blendId);
  const {
    submitForReview,
    submitForApproval,
    reviewBlend,
    approveBlend,
    isSubmitting,
  } = useBlendWorkflowMutations(blendId);

  // Menu states
  const [sectionAnchorEl, setSectionAnchorEl] = useState<null | HTMLElement>(null);
  const [approvalAnchorEl, setApprovalAnchorEl] = useState<null | HTMLElement>(null);

  // Dialog & execution states
  const [commentDialogOpen, setCommentDialogOpen] = useState(false);
  const [activeAction, setActiveAction] = useState<'SELF_REVIEW' | 'SELF_APPROVE' | null>(null);

  const handleOpenSectionMenu = (e: React.MouseEvent<HTMLButtonElement>) => {
    setSectionAnchorEl(e.currentTarget);
  };

  const handleCloseSectionMenu = () => setSectionAnchorEl(null);

  const handleOpenApprovalMenu = (e: React.MouseEvent<HTMLButtonElement>) => {
    setApprovalAnchorEl(e.currentTarget);
  };

  const handleCloseApprovalMenu = () => setApprovalAnchorEl(null);

  const handleAddSection = (sectionTypeId: SectionType) => {
    handleCloseSectionMenu();
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

  const handleActionSelect = async (action: ApprovalActionType) => {
    handleCloseApprovalMenu();

    try {
      if (action === 'SEND_FOR_REVIEW') {
        const res = await submitForReview();
        onActionSuccess?.(res.message);
      } else if (action === 'SEND_FOR_APPROVE') {
        const res = await submitForApproval();
        onActionSuccess?.(res.message);
      } else if (action === 'SELF_REVIEW' || action === 'SELF_APPROVE') {
        setActiveAction(action);
        setCommentDialogOpen(true);
      }
    } catch (err) {
      console.error('Workflow action failed:', err);
    }
  };

  const handleCommentSubmit = async (comments: string) => {
    if (!activeAction) return;

    try {
      let res;
      if (activeAction === 'SELF_REVIEW') {
        res = await reviewBlend(comments);
      } else {
        res = await approveBlend(comments);
      }
      setCommentDialogOpen(false);
      onActionSuccess?.(res.message);
    } catch (err) {
      console.error('Submit comment failed:', err);
    } finally {
      setActiveAction(null);
    }
  };

  return (
    <Paper
      elevation={0}
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'flex-start',
        gap: 1,
        py: 0.5,
        px: 1,
        mt: 1,
        borderRadius: 1,
        backgroundColor: 'action.hover',
      }}
    >
      {/* Add Section Button */}
      <Box sx={{ display: 'flex', gap: 1 }}>
        <Button
          size="small"
          variant="outlined"
          startIcon={<AddIcon />}
          onClick={handleOpenSectionMenu}
          disabled={isSaving || isSubmitting}
        >
          {isSaving ? 'Adding...' : 'Add Section'}
        </Button>
        <Menu
          anchorEl={sectionAnchorEl}
          open={Boolean(sectionAnchorEl)}
          onClose={handleCloseSectionMenu}
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

      {/* Approval Actions Dropdown */}
      <Box>
        <Button
          size="small"
          variant="contained"
          color="primary"
          endIcon={<KeyboardArrowDownIcon />}
          onClick={handleOpenApprovalMenu}
          disabled={isSubmitting}
        >
          {isSubmitting ? 'Processing...' : 'Approval Actions'}
        </Button>
        <Menu
          anchorEl={approvalAnchorEl}
          open={Boolean(approvalAnchorEl)}
          onClose={handleCloseApprovalMenu}
          transformOrigin={{ horizontal: 'left', vertical: 'top' }}
          anchorOrigin={{ horizontal: 'left', vertical: 'bottom' }}
        >
          {APPROVAL_OPTIONS.map((option) => (
            <React.Fragment key={option.type}>
              <MenuItem onClick={() => handleActionSelect(option.type)}>
                <ListItemIcon>{option.icon}</ListItemIcon>
                <ListItemText primary={option.label} />
              </MenuItem>
              {option.hasDividerAfter && <Divider />}
            </React.Fragment>
          ))}
        </Menu>
      </Box>

      {/* Comment Modal for Self Review / Self Approve */}
      <CommentDialog
        open={commentDialogOpen}
        title={activeAction === 'SELF_REVIEW' ? 'Self Review Blend' : 'Self Approve Blend'}
        loading={isSubmitting}
        onClose={() => setCommentDialogOpen(false)}
        onSubmit={handleCommentSubmit}
      />
    </Paper>
  );
};