import React, { createContext, useContext } from 'react';

export interface ConfirmOptions {
  title: string;
  message: React.ReactNode;
  confirmText?: string;
  cancelText?: string;
  confirmColor?: 'error' | 'primary' | 'secondary' | 'warning' | 'info' | 'success';
  onConfirm: () => Promise<void> | void;
}

export interface DialogContextType {
  confirm: (options: ConfirmOptions) => void;
  closeDialog: () => void;
}

export const DialogContext = createContext<DialogContextType | undefined>(undefined);

export const useConfirm = (): DialogContextType['confirm'] => {
  const context = useContext(DialogContext);
  if (!context) {
    throw new Error('useConfirm must be used within a DialogProvider');
  }
  return context.confirm;
};