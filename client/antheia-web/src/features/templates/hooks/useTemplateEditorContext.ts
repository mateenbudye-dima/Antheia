import { useContext } from 'react';
import { TemplateEditorContext } from '../context/editorContextInstance';
import type { TemplateEditorContextType } from '../types/editor.types';

export const useTemplateEditorContext = (): TemplateEditorContextType => {
  const context = useContext(TemplateEditorContext);
  if (!context) {
    throw new Error(
      'useTemplateEditorContext must be used within a TemplateEditorProvider'
    );
  }
  return context;
};