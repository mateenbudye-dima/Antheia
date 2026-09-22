// src/features/templates/components/SectionTree.tsx
import React, { useState } from 'react';
import {
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Collapse,
  Paper,
  Typography,
  Box,
} from '@mui/material';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';
import FolderIcon from '@mui/icons-material/Folder';
import ArticleIcon from '@mui/icons-material/Article';

export interface SectionNode {
  id: string;
  title: string;
  children?: SectionNode[];
}

interface SectionTreeProps {
  sections: SectionNode[];
  selectedSectionId: string | null;
  onSelectSection: (sectionId: string) => void;
}

interface TreeItemProps {
  node: SectionNode;
  level?: number;
  selectedSectionId: string | null;
  onSelectSection: (sectionId: string) => void;
}

const TreeItem: React.FC<TreeItemProps> = ({
  node,
  level = 0,
  selectedSectionId,
  onSelectSection,
}) => {
  const hasChildren = node.children && node.children.length > 0;
  const [open, setOpen] = useState(true);

  const handleClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (hasChildren) {
      setOpen((prev) => !prev);
    }
    onSelectSection(node.id);
  };

  const isSelected = selectedSectionId === node.id;

  return (
    <>
      <ListItemButton
        selected={isSelected}
        onClick={handleClick}
        sx={{
          pl: level * 2 + 2,
          py: 0.75,
          borderRadius: 1,
          mb: 0.5,
          '&.Mui-selected': {
            backgroundColor: 'primary.light',
            color: 'primary.contrastText',
            '& .MuiListItemIcon-root': {
              color: 'primary.contrastText',
            },
            '&:hover': {
              backgroundColor: 'primary.main',
            },
          },
        }}
      >
        <ListItemIcon sx={{ minWidth: 32 }}>
          {hasChildren ? <FolderIcon fontSize="small" /> : <ArticleIcon fontSize="small" />}
        </ListItemIcon>
        <ListItemText
          primary={node.title}
          sx={{ primaryTypographyProps: { variant: 'body2', fontWeight: isSelected ? 'bold' : 'normal' } }}
        />
        {hasChildren ? open ? <ExpandLess fontSize="small" /> : <ExpandMore fontSize="small" /> : null}
      </ListItemButton>

      {hasChildren && (
        <Collapse in={open} timeout="auto" unmountOnExit>
          <List component="div" disablePadding>
            {node.children!.map((child) => (
              <TreeItem
                key={child.id}
                node={child}
                level={level + 1}
                selectedSectionId={selectedSectionId}
                onSelectSection={onSelectSection}
              />
            ))}
          </List>
        </Collapse>
      )}
    </>
  );
};

export const SectionTree: React.FC<SectionTreeProps> = ({
  sections,
  selectedSectionId,
  onSelectSection,
}) => {
  return (
    <Paper
      variant="outlined"
      sx={{
        p: 1.5,
        height: '100%',
        minHeight: 'calc(100vh - 180px)',
        overflowY: 'auto',
      }}
    >
      <Box sx={{ px: 1, pb: 1, borderBottom: '1px solid', borderColor: 'divider', mb: 1 }}>
        <Typography variant="subtitle2" color="text.secondary" sx={{ fontWeight: 'bold' }}>
          TEMPLATE SECTIONS
        </Typography>
      </Box>
      <List component="nav" dense>
        {sections.map((node) => (
          <TreeItem
            key={node.id}
            node={node}
            selectedSectionId={selectedSectionId}
            onSelectSection={onSelectSection}
          />
        ))}
      </List>
    </Paper>
  );
};