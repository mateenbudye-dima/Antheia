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
  IconButton,
} from '@mui/material';
import ExpandLess from '@mui/icons-material/ExpandLess';
import ExpandMore from '@mui/icons-material/ExpandMore';
import FolderIcon from '@mui/icons-material/Folder';
import FolderOpenIcon from '@mui/icons-material/FolderOpen';
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
  const hasChildren = Boolean(node.children && node.children.length > 0);
  const [open, setOpen] = useState(true);

  const isSelected = selectedSectionId === node.id;

  // Handles selecting the section item
  const handleItemClick = () => {
    onSelectSection(node.id);
  };

  // Handles expanding/collapsing folders independently without selecting the item
  const handleToggleExpand = (e: React.MouseEvent) => {
    e.stopPropagation();
    setOpen((prev) => !prev);
  };

  return (
    <>
      <ListItemButton
        selected={isSelected}
        onClick={handleItemClick}
        aria-selected={isSelected}
        aria-expanded={hasChildren ? open : undefined}
        sx={{
          pl: level * 2 + 1.5,
          py: 0.75,
          borderRadius: 1,
          mb: 0.5,
          transition: 'background-color 0.15s ease',
          '&.Mui-selected': {
            backgroundColor: 'primary.main',
            color: 'primary.contrastText',
            '& .MuiListItemIcon-root': {
              color: 'primary.contrastText',
            },
            '& .MuiIconButton-root': {
              color: 'primary.contrastText',
            },
            '&:hover': {
              backgroundColor: 'primary.dark',
            },
          },
        }}
      >
        <ListItemIcon sx={{ minWidth: 32 }}>
          {hasChildren ? (
            open ? (
              <FolderOpenIcon fontSize="small" />
            ) : (
              <FolderIcon fontSize="small" />
            )
          ) : (
            <ArticleIcon fontSize="small" />
          )}
        </ListItemIcon>

        <ListItemText
          primary={node.title}
          slotProps={{
            primary: {
              variant: 'body2',
              sx: {
                fontWeight: isSelected ? 600 : 400,
              },
              noWrap: true,
              title: node.title, // Native tooltip on overflow hover
            },
          }}
        />

        {hasChildren && (
          <IconButton
            size="small"
            onClick={handleToggleExpand}
            sx={{ p: 0.25, ml: 0.5 }}
            aria-label={open ? 'Collapse section' : 'Expand section'}
          >
            {open ? <ExpandLess fontSize="small" /> : <ExpandMore fontSize="small" />}
          </IconButton>
        )}
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
        display: 'flex',
        flexDirection: 'column',
      }}
    >
      <Box
        sx={{
          px: 1,
          pb: 1,
          borderBottom: '1px solid',
          borderColor: 'divider',
          mb: 1,
        }}
      >
        <Typography
          variant="subtitle2"
          color="text.secondary"
          sx={{ fontWeight: 'bold', letterSpacing: 0.5 }}
        >
          TEMPLATE SECTIONS
        </Typography>
      </Box>

      {sections.length === 0 ? (
        <Typography
          variant="caption"
          color="text.secondary"
          sx={{ px: 1, py: 2, fontStyle: 'italic' }}
        >
          No sections available
        </Typography>
      ) : (
        <List component="nav" dense disablePadding sx={{ flexGrow: 1 }}>
          {sections.map((node) => (
            <TreeItem
              key={node.id}
              node={node}
              selectedSectionId={selectedSectionId}
              onSelectSection={onSelectSection}
            />
          ))}
        </List>
      )}
    </Paper>
  );
};