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
  Chip,
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
import CloseIcon from '@mui/icons-material/Close';

import { BLEND_STATUS_LABELS, BlendStatus, getStatusColor, SectionType } from '../types/blend.types';
import { useBlendMutations } from '../hooks/useBlendMutations';
import { useBlendWorkflowMutations } from '../hooks/useBlendWorkflowMutations';
import type { CreateSectionPayload } from '../api/blendsApi';
import { CommentDialog } from './CommentDialog';

export type SubmissionAction =
  | 'SEND_FOR_REVIEW'
  | 'SEND_FOR_APPROVE'
  | 'SELF_REVIEW'
  | 'SELF_APPROVE'
  | 'CANCEL_SUBMISSION';
export type DecisionAction =
  | 'REVIEW'
  | 'APPROVE'
  | 'REJECT';

interface SubmissionOption {
  type: SubmissionAction;
  label: string;
  icon: React.ReactNode;
  hasDividerAfter?: boolean;
}

interface DecisionOption {
  type: DecisionAction;
  label: string;
  icon: React.ReactNode;
  hasDividerAfter?: boolean;
}

interface SectionMenuOption {
  type: SectionType;
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


const SUBMISSION_OPTIONS: SubmissionOption[] = [
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
  {
    type: 'CANCEL_SUBMISSION',
    label: 'Cancel Submission',
    icon: <AssignmentTurnedInIcon fontSize="small" />,
  },
];

const DECISION_OPTIONS: DecisionOption[] = [
  {
    type: 'REVIEW',
    label: 'Review',
    icon: <CheckCircleOutlineIcon fontSize="small" />,
  },
  {
    type: 'APPROVE',
    label: 'Approve',
    icon: <AssignmentTurnedInIcon fontSize="small" />,
  },
  {
    type: 'REJECT',
    label: 'Reject',
    icon: <CloseIcon fontSize="small" />,
  },
];

interface EditorToolbarProps {
  blendId: number;
  blendStatus: BlendStatus;
  blendCode: string;
  readOnly: boolean;
  isAuthor: boolean;
  onActionSuccess?: (message: string) => void;
}

export const EditorToolbar: React.FC<EditorToolbarProps> = ({
  blendId,
  blendStatus,
  blendCode,
  readOnly,
  onActionSuccess,
  isAuthor,
}) => {
  const { addSection, isSaving } = useBlendMutations(blendId);
  const {
    submitForReview,
    submitForApproval,
    reviewBlend,
    approveBlend,
    rejectBlend,
    cancelSubmission,
    isSubmitting,
  } = useBlendWorkflowMutations(blendId);

  // Menu states
  const [sectionAnchorEl, setSectionAnchorEl] = useState<null | HTMLElement>(null);
  const [submissionAnchorEl, setSubmissionAnchorEl] = useState<null | HTMLElement>(null);
  const [decisionAnchorEl, setDecisionAnchorEl] = useState<null | HTMLElement>(null);

  // Dialog & execution states
  const [commentDialogOpen, setCommentDialogOpen] = useState(false);
  const [activeAction, setActiveAction] = useState<'SELF_REVIEW' | 'SELF_APPROVE' | 'CANCEL_SUBMISSION' | null>(null);
  const [decisionActiveAction, setDecisionActiveAction] = useState<'REVIEW' | 'APPROVE' | 'REJECT' | null>(null);

  const handleOpenSectionMenu = (e: React.MouseEvent<HTMLButtonElement>) => {
    setSectionAnchorEl(e.currentTarget);
  };

  const handleCloseSectionMenu = () => setSectionAnchorEl(null);

  const handleOpenSubmissionMenu = (e: React.MouseEvent<HTMLButtonElement>) => {
    setSubmissionAnchorEl(e.currentTarget);
  };

  const handleCloseDecisionMenu = () => setDecisionAnchorEl(null);

  const handleOpenDecisionMenu = (e: React.MouseEvent<HTMLButtonElement>) => {
    setDecisionAnchorEl(e.currentTarget);
  };

  const handleCloseSubmissionMenu = () => setSubmissionAnchorEl(null);

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

  const handleActionSelect = async (action: SubmissionAction) => {
    handleCloseSubmissionMenu();

    try {
      if (action === 'SEND_FOR_REVIEW') {
        const res = await submitForReview();
        onActionSuccess?.(res.message);
      } else if (action === 'SEND_FOR_APPROVE') {
        const res = await submitForApproval();
        onActionSuccess?.(res.message);
      } else if (action === 'SELF_REVIEW' || action === 'SELF_APPROVE' || action === 'CANCEL_SUBMISSION') {
        setActiveAction(action);
        setCommentDialogOpen(true);
      }
    } catch (err) {
      console.error('Workflow action failed:', err);
    }
  };

  const handleDecisionActionSelect = async (action: DecisionAction) => {
    handleCloseDecisionMenu();

    try {
      setDecisionActiveAction(action);
      setCommentDialogOpen(true);
    } catch (err) {
      console.error('Workflow action failed:', err);
    }
  };

  const handleCommentSubmit = async (comments: string) => {
    if (!activeAction && !decisionActiveAction) return;

    try {
      let res;
        if (activeAction === 'SELF_REVIEW' || decisionActiveAction === 'REVIEW') {
          res = await reviewBlend(comments);
        }else if (activeAction === 'SELF_APPROVE' || decisionActiveAction === 'APPROVE') {
          res = await approveBlend(comments); // Assuming reviewBlend handles rejection as well
        } 
        else if (decisionActiveAction === 'REJECT') {
          res = await rejectBlend(comments); // Assuming reviewBlend handles rejection as well
        } else {
          res = await cancelSubmission(comments);        
        }
        
        setCommentDialogOpen(false);
        onActionSuccess?.(res.message);
    } catch (err) {
      console.error('Submit comment failed:', err);
    } finally {
      setActiveAction(null);
      setDecisionActiveAction(null);
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
      {
        !readOnly && 
        (
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
        )
      }
      

      {/* Submission Actions Dropdown */}
      {blendStatus !== BlendStatus.Approved && isAuthor && (
        <Box>
          <Button
            size="small"
            variant="outlined"
            color="primary"
          endIcon={<KeyboardArrowDownIcon />}
          onClick={handleOpenSubmissionMenu}
          disabled={isSubmitting}
        >
          {isSubmitting ? 'Processing...' : 'Submit Actions'}
        </Button>
        <Menu
          anchorEl={submissionAnchorEl}
          open={Boolean(submissionAnchorEl)}
          onClose={handleCloseSubmissionMenu}
          transformOrigin={{ horizontal: 'left', vertical: 'top' }}
          anchorOrigin={{ horizontal: 'left', vertical: 'bottom' }}
        >
          {SUBMISSION_OPTIONS.map((option) => 
            ((option.type === 'SEND_FOR_REVIEW' && blendStatus === BlendStatus.Draft)|| 
              (option.type === 'SEND_FOR_APPROVE'
                 && (blendStatus === BlendStatus.Draft || blendStatus === BlendStatus.Reviewed)) ||
              (option.type === 'SELF_REVIEW' 
                 && (blendStatus === BlendStatus.Draft || blendStatus === BlendStatus.SubmittedForReview)) ||
              (option.type === 'SELF_APPROVE'
                 && (blendStatus === BlendStatus.Draft || blendStatus === BlendStatus.Reviewed || blendStatus === BlendStatus.SubmittedForApproval)) ||
              (option.type === 'CANCEL_SUBMISSION'
                 && (blendStatus === BlendStatus.SubmittedForReview || blendStatus === BlendStatus.SubmittedForApproval))
              )
            && (
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
      )}

      {/* Decision Actions Dropdown */}
      {!isAuthor 
        && (blendStatus === BlendStatus.SubmittedForApproval || blendStatus === BlendStatus.SubmittedForReview) 
        && (
        <Box>
          <Button
            size="small"
            variant="outlined"
            color="secondary"
          endIcon={<KeyboardArrowDownIcon />}
          onClick={handleOpenDecisionMenu}
          disabled={isSubmitting}
        >
          {isSubmitting ? 'Processing...' : 'Audit Actions'}
          </Button>
          <Menu
            anchorEl={decisionAnchorEl}
            open={Boolean(decisionAnchorEl)}
            onClose={handleCloseDecisionMenu}
            transformOrigin={{ horizontal: 'left', vertical: 'top' }}
            anchorOrigin={{ horizontal: 'left', vertical: 'bottom' }}
          >
            {DECISION_OPTIONS.map((option) => 
              ((blendStatus === BlendStatus.SubmittedForApproval && option.type === 'APPROVE') ||
              (blendStatus === BlendStatus.SubmittedForReview && option.type === 'REVIEW')
              || (option.type === 'REJECT'))
              && (
              <React.Fragment key={option.type}>
                <MenuItem onClick={() => handleDecisionActionSelect(option.type)}>
                  <ListItemIcon>{option.icon}</ListItemIcon>
                  <ListItemText primary={option.label} />
                </MenuItem>
                {option.hasDividerAfter && <Divider />}
              </React.Fragment>
            ))}
          </Menu>
        </Box>
      )}
      
      <Chip
        label={blendStatus !== undefined ? BLEND_STATUS_LABELS[blendStatus] : 'Draft'}
        color={getStatusColor(blendStatus)}
        size="small"
        variant="outlined"
        sx={{ fontWeight: 'bold', ml: 'auto' }}
      />

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