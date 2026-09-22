// src/features/templates/types/editor.types.ts
export type ViewMode = 'split' | 'all';

export interface EditorState {
  selectedSectionId: string;
  viewMode: ViewMode;
}

export type EditorAction =
  | { type: 'SET_SELECTED_SECTION'; payload: string }
  | { type: 'SET_VIEW_MODE'; payload: ViewMode };

export interface TemplateEditorContextType {
  state: EditorState;
  dispatch: React.Dispatch<EditorAction>;
  setSelectedSection: (sectionId: string) => void;
  setViewMode: (mode: ViewMode) => void;
}