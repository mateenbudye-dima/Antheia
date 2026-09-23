import apiClient from "../../../shared/api/apiClient";
import type { EvaluationItem, FullTemplateResponse, Ingredient, PreparationMethod } from "../types/template.types";

export interface UpdateTemplateHeaderDto {
  title: string;
  objective: string;
  description: string;
}
export interface CreateSectionPayload {
  sectionTitle: string;
  sectionTypeId: 1 | 2 | 3; // 1: Ingredients, 2: Prep, 3: Evaluation
}
// DTOs for create & update payloads
export interface CreateIngredientPayload {
  sectionId: number;
  name: string;
  type?: string;
  ratio?: number;
  quantity?: number;
}

export interface UpdateIngredientPayload {
  name: string;
  type?: string;
  ratio?: number;
  quantity?: number;
}

export interface UpdatePrepMethodPayload {
  additionSequence?: string | null;
  mixingSpeed?: string | null;
  mixingTime?: string | null;
  temperature?: string | null;
}

export interface CreateEvaluationPayload {
  sectionId: number;
  evaluationParameter: string;
  specification: string;
  result?: string | null;
  status?: string | null;
}

export interface UpdateEvaluationPayload {
  evaluationParameter: string;
  specification?: string;
  result?: string | null;
  status?: string | null;
}

export const templatesApi = {
    
  // ==========================================
  // 1. FULL TEMPLATE READ
  // ==========================================
  getFullTemplate: async (templateId: number): Promise<FullTemplateResponse> => {
    const response = await apiClient.get<FullTemplateResponse>(`/templates/${templateId}`);
    return response.data;
  },

  // ==========================================
  // 2. TEMPLATE HEADER UPDATE
  // ==========================================
  updateTemplateHeader: async (templateId: number, payload: UpdateTemplateHeaderDto): Promise<void> => {
    await apiClient.patch(`/templates/${templateId}/header`, payload);
  },

  addSection: async (templateId: number, payload: CreateSectionPayload): Promise<void> => {
    await apiClient.post(`/templates/${templateId}/sections`, payload);
  },

  deleteSection: async (sectionId: number): Promise<void> => {
    await apiClient.delete(`/templates/sections/${sectionId}`);
  },
  
  // ==========================================
  // 3. INGREDIENTS SECTION
  // ==========================================
  addIngredient: async (payload: CreateIngredientPayload): Promise<Ingredient> => {
    const response = await apiClient.post<Ingredient>('/templates/ingredients', payload);
    return response.data;
  },

  updateIngredient: async (id: number, payload: UpdateIngredientPayload): Promise<void> => {
    await apiClient.patch(`/templates/ingredients/${id}`, payload);
  },

  deleteIngredient: async (id: number): Promise<void> => {
    await apiClient.delete(`/templates/ingredients/${id}`);
  },

  // ==========================================
  // 4. PREPARATION METHOD SECTION
  // ==========================================
  updatePrepMethod: async (prepId: number, payload: UpdatePrepMethodPayload): Promise<PreparationMethod> => {
    const response = await apiClient.patch<PreparationMethod>(`/templates/prep-methods/${prepId}`, payload);
    return response.data;
  },

  // ==========================================
  // 5. EVALUATION SECTION
  // ==========================================
  addEvaluation: async (payload: CreateEvaluationPayload): Promise<EvaluationItem> => {
    const response = await apiClient.post<EvaluationItem>('/templates/evaluations', payload);
    return response.data;
  },

  updateEvaluation: async (evaluationId: number, payload: UpdateEvaluationPayload): Promise<void> => {
    await apiClient.patch(`/templates/evaluations/${evaluationId}`, payload);
  },

  deleteEvaluation: async (evaluationId: number): Promise<void> => {
    await apiClient.delete(`/templates/evaluations/${evaluationId}`);
  },
};