import apiClient from "../../../shared/api/apiClient";
import type { EvaluationItem, FullBlendResponse, Ingredient, PreparationMethod, SectionType } from "../types/blend.types";

export interface UpdateBlendHeaderDto {
  title: string;
  objective: string;
  description: string;
}
export interface CreateSectionPayload {
  sectionTitle: string;
  sectionTypeId: SectionType;
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

export const blendsApi = {
    
  // ==========================================
  // 1. FULL BLEND READ
  // ==========================================
  getFullBlend: async (blendId: number): Promise<FullBlendResponse> => {
    const response = await apiClient.get<FullBlendResponse>(`/blends/${blendId}`);
    return response.data;
  },

  // ==========================================
  // 2. BLEND HEADER UPDATE
  // ==========================================
  updateBlendHeader: async (blendId: number, payload: UpdateBlendHeaderDto): Promise<void> => {
    await apiClient.patch(`/blends/${blendId}/header`, payload);
  },

  addSection: async (blendId: number, payload: CreateSectionPayload): Promise<void> => {
    await apiClient.post(`/blends/${blendId}/sections`, payload);
  },

  deleteSection: async (sectionId: number): Promise<void> => {
    await apiClient.delete(`/blends/sections/${sectionId}`);
  },
  
  // ==========================================
  // 3. INGREDIENTS SECTION
  // ==========================================
  addIngredient: async (payload: CreateIngredientPayload): Promise<Ingredient> => {
    const response = await apiClient.post<Ingredient>('/blends/ingredients', payload);
    return response.data;
  },

  updateIngredient: async (id: number, payload: UpdateIngredientPayload): Promise<void> => {
    await apiClient.patch(`/blends/ingredients/${id}`, payload);
  },

  deleteIngredient: async (id: number): Promise<void> => {
    await apiClient.delete(`/blends/ingredients/${id}`);
  },

  // ==========================================
  // 4. PREPARATION METHOD SECTION
  // ==========================================
  updatePrepMethod: async (prepId: number, payload: UpdatePrepMethodPayload): Promise<PreparationMethod> => {
    const response = await apiClient.patch<PreparationMethod>(`/blends/prep-methods/${prepId}`, payload);
    return response.data;
  },

  // ==========================================
  // 5. EVALUATION SECTION
  // ==========================================
  addEvaluation: async (payload: CreateEvaluationPayload): Promise<EvaluationItem> => {
    const response = await apiClient.post<EvaluationItem>('/blends/evaluations', payload);
    return response.data;
  },

  updateEvaluation: async (evaluationId: number, payload: UpdateEvaluationPayload): Promise<void> => {
    await apiClient.patch(`/blends/evaluations/${evaluationId}`, payload);
  },

  deleteEvaluation: async (evaluationId: number): Promise<void> => {
    await apiClient.delete(`/blends/evaluations/${evaluationId}`);
  },
};