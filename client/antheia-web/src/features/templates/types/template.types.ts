export const SectionType = {
  Ingredients: 1,
  PreparationMethod: 2,
  Evaluation: 3,
} as const;
export type SectionType = typeof SectionType[keyof typeof SectionType];

export interface Ingredient {
  sectionIngredientId: number;
  name: string;
  type: string;
  ratio: number;
  quantity: number;
}

export interface PreparationMethod {
  preparationId: number;
  additionSequence: string | null;
  mixingSpeed: string | null;
  mixingTime: string | null;
  temperature: string | null;
}

export interface EvaluationItem {
  evaluationId: number;
  evaluationParameter: string;
  result: string | null;
  specification: string;
  status: string | null;
}

export interface Section {
  sectionId: number;
  sectionTypeId: SectionType;
  sectionTitle: string;
  sectionOrder: number;
  ingredients: Ingredient[];
  preparationMethod: PreparationMethod | null;
  evaluations: EvaluationItem[];
}

export interface FullTemplateResponse {
  templateId: number;
  title: string;
  objective: string;
  description: string;
  isPublished: boolean;
  sections: Section[];
}

