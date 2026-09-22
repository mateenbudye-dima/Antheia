import React, { useReducer, type ReactNode } from 'react';
import {
  TemplateEditorContext,
  INITIAL_EDITOR_STATE,
  editorReducer,
} from './editorContextInstance';

export const TemplateEditorProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  const [state, dispatch] = useReducer(editorReducer, INITIAL_EDITOR_STATE);

  const setSelectedSection = (sectionId: string) => {
    dispatch({ type: 'SET_SELECTED_SECTION', payload: sectionId });
  };

  const setViewMode = (mode: 'split' | 'all') => {
    dispatch({ type: 'SET_VIEW_MODE', payload: mode });
  };

  return (
    <TemplateEditorContext.Provider
      value={{ state, dispatch, setSelectedSection, setViewMode }}
    >
      {children}
    </TemplateEditorContext.Provider>
  );
};