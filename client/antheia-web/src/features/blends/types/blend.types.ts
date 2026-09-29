export const SectionType = {
  Ingredients: 1,
  PreparationMethod: 2,
  Evaluation: 3,
} as const;
export type SectionType = typeof SectionType[keyof typeof SectionType];

export const IngredientType = {
  Unknown: 0,
  ActiveIngredient: 1,
  Solvent: 2,
  EmulsifierBlend: 3,
  Dispersant: 4,
  WettingAgent: 5,
  OtherAdditives: 100,
} as const;

export type IngredientType = (typeof IngredientType)[keyof typeof IngredientType];
// Map enum values to human-readable labels
export const IngredientTypeLabels: Record<IngredientType, string> = {
  [IngredientType.Unknown]: 'Unknown',
  [IngredientType.ActiveIngredient]: 'Active Ingredient',
  [IngredientType.Solvent]: 'Solvent',
  [IngredientType.EmulsifierBlend]: 'Emulsifier Blend',
  [IngredientType.Dispersant]: 'Dispersant',
  [IngredientType.WettingAgent]: 'Wetting Agent',
  [IngredientType.OtherAdditives]: 'Other Additives',
};

// Array of options for mapping inside <Select>
export const INGREDIENT_TYPE_OPTIONS = Object.entries(IngredientTypeLabels).map(
  ([value, label]) => ({
    value: Number(value) as IngredientType,
    label,
  })
);

export interface Ingredient {
  sectionIngredientId: number;
  name: string;
  type: IngredientType;
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

export interface FullBlendResponse {
  blendId: number;
  code: string;
  trialNumber: string;
  objective: string;
  description: string;
  isPublished: boolean;
  sections: Section[];
}

