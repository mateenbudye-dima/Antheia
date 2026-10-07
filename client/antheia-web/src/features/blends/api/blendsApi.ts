import apiClient from "../../../shared/api/apiClient";
import type { BlendStatus, EvaluationItem, FullBlendResponse, Ingredient, IngredientType, PreparationMethod, SectionType } from "../types/blend.types";



export interface BlendItem {
  blendId: number;
  code: string;
  trialNumber?: string | null;
  objective: string | null;
  updatedDate: string;
  status: BlendStatus | null;
  isPublished: boolean | null;
  createdBy: string;
}
export interface UpdateBlendHeaderDto {
  code: string;
  objective: string;
  description: string;
  trialNumber?: string;
}
export interface CreateSectionPayload {
  sectionTitle: string;
  sectionTypeId: SectionType;
  
  blendCode: string;
}
// DTOs for create & update payloads
export interface CreateIngredientPayload {
  sectionId: number;
  name: string;
  type: IngredientType;
  ratio?: number;
  quantity?: number;

  sectionTitle: string;
  blendId: number;
  blendCode?: string;
}

export interface UpdateIngredientPayload {
  name: string;
  type: IngredientType;
  ratio?: number;
  quantity?: number;

  sectionId: number;
  sectionTitle: string;
  blendId: number;
  blendCode?: string;
}

export interface UpdatePrepMethodPayload {
  additionSequence?: string | null;
  mixingSpeed?: string | null;
  mixingTime?: string | null;
  temperature?: string | null;

  sectionId: number;
  sectionTitle: string;
  blendId: number;
  blendCode?: string;
}

export interface CreateEvaluationPayload {
  sectionId: number;
  evaluationParameter: string;
  specification: string;
  result?: string | null;
  status?: string | null;

  sectionTitle: string;
  blendId: number;
  blendCode?: string;
}

export interface UpdateEvaluationPayload {
  evaluationParameter: string;
  specification?: string;
  result?: string | null;
  status?: string | null;

  sectionId: number;
  sectionTitle: string;
  blendId: number;
  blendCode?: string;
}

export const blendsApi = {
    
  getBlends: async (): Promise<BlendItem[]> => {
    const { data } = await apiClient.get<BlendItem[]>('/blends');
    return data;
  },

  createDraft: async (): Promise<{ blendId: number }> => {
    const { data } = await apiClient.post<{ blendId: number }>('/blends/draft');
    return data;
  },

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

  deleteSection: async (blendId: number, sectionId: number): Promise<void> => {
    await apiClient.delete(`/blends/${blendId}/sections/${sectionId}`);
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

  submitBlendForReview : async (id: number): Promise<{ message: string }> => {
    const response = await apiClient.post(`/blends/${id}/submit-for-review`);
    return response.data;
  },

  submitBlendForApproval : async (id: number): Promise<{ message: string }> => {
    const response = await apiClient.post(`/blends/${id}/submit-for-approval`);
    return response.data;
  },

  reviewBlend : async (id: number, comments: string): Promise<{ message: string }> => {
    const response = await apiClient.post(`/blends/${id}/review`, JSON.stringify(comments), {
      headers: { 'Content-Type': 'application/json' },
    });
    return response.data;
  },

  approveBlend : async (id: number, comments: string): Promise<{ message: string }> => {
    const response = await apiClient.post(`/blends/${id}/approve`, JSON.stringify(comments), {
      headers: { 'Content-Type': 'application/json' },
    });
    return response.data;
  },
};