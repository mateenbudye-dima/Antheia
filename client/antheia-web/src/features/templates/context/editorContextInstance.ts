import { createContext } from 'react';
import type {
  EditorState,
  EditorAction,
  TemplateEditorContextType,
} from '../types/editor.types';

export const INITIAL_EDITOR_STATE: EditorState = {
  selectedSectionId: 'header',
  viewMode: 'split',
};

export function editorReducer(
  state: EditorState,
  action: EditorAction
): EditorState {
  switch (action.type) {
    case 'SET_SELECTED_SECTION':
      return { ...state, selectedSectionId: action.payload };
    case 'SET_VIEW_MODE':
      return { ...state, viewMode: action.payload };
    default:
      return state;
  }
}

export const TemplateEditorContext = createContext<
  TemplateEditorContextType | undefined
>(undefined);