import { useContext } from 'react';
import { BlendEditorContext } from '../context/editorContextInstance';
import type { BlendEditorContextType } from '../types/editor.types';

export const useBlendEditorContext = (): BlendEditorContextType => {
  const context = useContext(BlendEditorContext);
  if (!context) {
    throw new Error(
      'useBlendEditorContext must be used within a BlendEditorProvider'
    );
  }
  return context;
};