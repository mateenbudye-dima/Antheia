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
  evaluationParameterType: number;
  result: string | null;
  specification: string;
  status: string | null;
}

export interface Section {
  sectionId: number;
  sectionTypeId: number;
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

