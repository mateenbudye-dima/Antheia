// src/shared/layouts/Layout.tsx
import React from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import {
  AppBar,
  Box,
  CssBaseline,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography,
  Button,
  Avatar,
  useTheme,
  Tooltip,
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import DashboardIcon from '@mui/icons-material/Dashboard';
import LogoutIcon from '@mui/icons-material/Logout';
import PersonIcon from '@mui/icons-material/Person';
import DescriptionIcon from '@mui/icons-material/Description';
import Brightness4Icon from '@mui/icons-material/Brightness4';
import Brightness7Icon from '@mui/icons-material/Brightness7';
import { useColorMode } from '../../context/ColorModeContext';
import { useAuth } from '../../features/auth/hooks/useAuth';
import { useLayout } from './LayoutContext';

const DRAWER_WIDTH = 260;

export const Layout: React.FC = () => {
  const theme = useTheme();
  const { toggleColorMode } = useColorMode();
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const { customSidebar, isMobileSidebarOpen, setIsMobileSidebarOpen, toggleMobileSidebar } = useLayout();

  const navItems = [
    { text: 'Dashboard', path: '/dashboard', icon: <DashboardIcon /> },
    { text: 'Templates', path: '/templates', icon: <DescriptionIcon /> },
    { text: 'Profile', path: '/profile', icon: <PersonIcon /> },
  ];

  const getCurrentTitle = () => {
    const currentItem = navItems.find((item) =>
      location.pathname.startsWith(item.path)
    );
    return currentItem ? currentItem.text : 'Antheia';
  };

  const isSelected = (path: string) => location.pathname.startsWith(path);

  // If a custom sidebar (SectionTree) is provided, render it. Otherwise, render main navigation.
  const drawerContent = (
    <Box sx={{ overflow: 'auto', height: '100%' }}>
      <Toolbar sx={{ justifyContent: 'center', py: 1 }}>
        <Typography variant="h6" color="primary" sx={{ fontWeight: 'bold' }}>
          {customSidebar ? 'Template Sections' : 'Antheia'}
        </Typography>
      </Toolbar>
      <Divider />
      {customSidebar ? (
        <Box sx={{ p: 2 }}>{customSidebar}</Box>
      ) : (
        <List>
          {navItems.map((item) => {
            const active = isSelected(item.path);
            return (
              <ListItem key={item.text} disablePadding>
                <ListItemButton
                  selected={active}
                  onClick={() => {
                    navigate(item.path);
                    setIsMobileSidebarOpen(false);
                  }}
                >
                  <ListItemIcon
                    sx={{ color: active ? 'primary.main' : 'inherit' }}
                  >
                    {item.icon}
                  </ListItemIcon>
                  <ListItemText
                    primary={
                      <Typography
                        variant="body1"
                        sx={{ fontWeight: active ? 'bold' : 'normal' }}
                      >
                        {item.text}
                      </Typography>
                    }
                  />
                </ListItemButton>
              </ListItem>
            );
          })}
        </List>
      )}
    </Box>
  );

  return (
    <Box sx={{ display: 'flex' }}>
      <CssBaseline />

      {/* Top Navbar */}
      <AppBar
        position="fixed"
        sx={{
          width: { md: `calc(100% - ${DRAWER_WIDTH}px)` },
          ml: { md: `${DRAWER_WIDTH}px` },
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            edge="start"
            onClick={toggleMobileSidebar}
            sx={{ mr: 2, display: { md: 'none' } }}
          >
            <MenuIcon />
          </IconButton>

          <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
            {getCurrentTitle()}
          </Typography>

          {user && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Avatar
                sx={{
                  bgcolor: 'secondary.main',
                  width: 32,
                  height: 32,
                  fontSize: '0.875rem',
                }}
              >
                {user.username ? user.username.charAt(0).toUpperCase() : 'U'}
              </Avatar>
              <Typography
                variant="body2"
                sx={{ display: { xs: 'none', sm: 'block' } }}
              >
                Welcome, <strong>{user.username}</strong>
              </Typography>

              <Tooltip
                title={`Switch to ${
                  theme.palette.mode === 'dark' ? 'light' : 'dark'
                } mode`}
              >
                <IconButton onClick={toggleColorMode} color="inherit" size="small">
                  {theme.palette.mode === 'dark' ? (
                    <Brightness7Icon />
                  ) : (
                    <Brightness4Icon />
                  )}
                </IconButton>
              </Tooltip>

              <Button
                color="inherit"
                startIcon={<LogoutIcon />}
                onClick={logout}
                size="small"
                sx={{ ml: 0.5 }}
              >
                Logout
              </Button>
            </Box>
          )}
        </Toolbar>
      </AppBar>

      {/* Navigation / Custom Sidebar Drawer */}
      <Box
        component="nav"
        sx={{ width: { md: DRAWER_WIDTH }, flexShrink: { md: 0 } }}
      >
        {/* Temporary Drawer for xs and sm screens */}
        <Drawer
          variant="temporary"
          open={isMobileSidebarOpen}
          onClose={ toggleMobileSidebar }
          ModalProps={{ keepMounted: true }}
          sx={{
            display: { xs: 'block', md: 'none' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: DRAWER_WIDTH,
            },
          }}
        >
          {drawerContent}
        </Drawer>

        {/* Permanent Drawer for Desktop Screens */}
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', md: 'block' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: DRAWER_WIDTH,
            },
          }}
          open
        >
          {drawerContent}
        </Drawer>
      </Box>

      {/* Main Page Content */}
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          width: { md: `calc(100% - ${DRAWER_WIDTH}px)` },
          minHeight: '100vh',
          backgroundColor: (theme) => theme.palette.background.default,
        }}
      >
        <Toolbar />
        <Outlet />
      </Box>
    </Box>
  );
};