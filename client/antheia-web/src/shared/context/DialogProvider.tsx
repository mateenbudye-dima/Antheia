import React, { useState, useCallback } from 'react';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { DialogContext, type ConfirmOptions } from './DialogContext';

export const DialogProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [dialogState, setDialogState] = useState<ConfirmOptions & { open: boolean }>({
    open: false,
    title: '',
    message: '',
    onConfirm: () => {},
  });
  const [isLoading, setIsLoading] = useState(false);

  const confirm = useCallback((options: ConfirmOptions) => {
    setDialogState({
      ...options,
      open: true,
    });
  }, []);

  const closeDialog = useCallback(() => {
    if (!isLoading) {
      setDialogState((prev) => ({ ...prev, open: false }));
    }
  }, [isLoading]);

  const handleConfirm = async () => {
    try {
      setIsLoading(true);
      await dialogState.onConfirm();
      setDialogState((prev) => ({ ...prev, open: false }));
    } catch (error) {
      console.error('Dialog action failed:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <DialogContext.Provider value={{ confirm, closeDialog }}>
      {children}
      <ConfirmDialog
        open={dialogState.open}
        title={dialogState.title}
        message={dialogState.message}
        confirmText={dialogState.confirmText}
        cancelText={dialogState.cancelText}
        confirmColor={dialogState.confirmColor}
        isLoading={isLoading}
        onConfirm={handleConfirm}
        onCancel={closeDialog}
      />
    </DialogContext.Provider>
  );
};