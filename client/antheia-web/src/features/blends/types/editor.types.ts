import { SectionType } from "./blend.types";

export type ViewMode = 'split' | 'all';

export interface EditorState {
  selectedSectionId: string;
  viewMode: ViewMode;
}

export type EditorAction =
  | { type: 'SET_SELECTED_SECTION'; payload: string }
  | { type: 'SET_VIEW_MODE'; payload: ViewMode };

export interface BlendEditorContextType {
  state: EditorState;
  dispatch: React.Dispatch<EditorAction>;
  setSelectedSection: (sectionId: string) => void;
  setViewMode: (mode: ViewMode) => void;
}

export const NodeType = {
  Header: 0,
  ...SectionType,
} as const;

export type NodeType = typeof NodeType[keyof typeof NodeType];