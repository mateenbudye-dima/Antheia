import { useMutation, useQueryClient } from '@tanstack/react-query';
import {
  templatesApi,
  type UpdateTemplateHeaderDto,
  type CreateIngredientPayload,
  type UpdateIngredientPayload,
  type UpdatePrepMethodPayload,
  type CreateEvaluationPayload,
  type UpdateEvaluationPayload,
} from '../api/templatesApi';

export const useTemplateMutations = (templateId: number) => {
  const queryClient = useQueryClient();

  // Centralized query cache invalidation helper
  const invalidateTemplate = () => {
    queryClient.invalidateQueries({ queryKey: ['template', templateId] });
    queryClient.invalidateQueries({ queryKey: ['templates'] });
  };

  // ==========================================
  // 1. HEADER MUTATION
  // ==========================================
  const updateHeaderMutation = useMutation({
    mutationFn: (payload: UpdateTemplateHeaderDto) =>
      templatesApi.updateTemplateHeader(templateId, payload),
    onSuccess: invalidateTemplate,
  });

  // ==========================================
  // 2. INGREDIENTS MUTATIONS
  // ==========================================
  const addIngredientMutation = useMutation({
    mutationFn: (payload: CreateIngredientPayload) =>
      templatesApi.addIngredient(payload),
    onSuccess: invalidateTemplate,
  });

  const updateIngredientMutation = useMutation({
    mutationFn: ({ id, payload }: { id: number; payload: UpdateIngredientPayload }) =>
      templatesApi.updateIngredient(id, payload),
    onSuccess: invalidateTemplate,
  });

  const deleteIngredientMutation = useMutation({
    mutationFn: (ingredientId: number) =>
      templatesApi.deleteIngredient(ingredientId),
    onSuccess: invalidateTemplate,
  });

  // ==========================================
  // 3. PREPARATION METHOD MUTATIONS
  // ==========================================
  const updatePrepMethodMutation = useMutation({
    mutationFn: ({
      prepId,
      payload,
    }: {
      prepId: number;
      payload: UpdatePrepMethodPayload;
    }) => templatesApi.updatePrepMethod(prepId, payload),
    onSuccess: invalidateTemplate,
  });

  // ==========================================
  // 4. EVALUATION MUTATIONS
  // ==========================================
  const addEvaluationMutation = useMutation({
    mutationFn: (payload: CreateEvaluationPayload) =>
      templatesApi.addEvaluation(payload),
    onSuccess: invalidateTemplate,
  });

  const updateEvaluationMutation = useMutation({
    mutationFn: ({
      evaluationId,
      payload,
    }: {
      evaluationId: number;
      payload: UpdateEvaluationPayload;
    }) => templatesApi.updateEvaluation(evaluationId, payload),
    onSuccess: invalidateTemplate,
  });

  const deleteEvaluationMutation = useMutation({
    mutationFn: (evaluationId: number) =>
      templatesApi.deleteEvaluation(evaluationId),
    onSuccess: invalidateTemplate,
  });

  return {
    // ------------------------------------------
    // Header
    // ------------------------------------------
    updateHeader: updateHeaderMutation.mutate,
    updateHeaderAsync: updateHeaderMutation.mutateAsync,

    // ------------------------------------------
    // Ingredients
    // ------------------------------------------
    addIngredient: addIngredientMutation.mutate,
    addIngredientAsync: addIngredientMutation.mutateAsync,
    updateIngredient: updateIngredientMutation.mutate,
    updateIngredientAsync: updateIngredientMutation.mutateAsync,
    deleteIngredient: deleteIngredientMutation.mutate,
    deleteIngredientAsync: deleteIngredientMutation.mutateAsync,

    // ------------------------------------------
    // Preparation Methods
    // ------------------------------------------
    updatePrepMethod: updatePrepMethodMutation.mutate,
    updatePrepMethodAsync: updatePrepMethodMutation.mutateAsync,

    // ------------------------------------------
    // Evaluations
    // ------------------------------------------
    addEvaluation: addEvaluationMutation.mutate,
    addEvaluationAsync: addEvaluationMutation.mutateAsync,
    updateEvaluation: updateEvaluationMutation.mutate,
    updateEvaluationAsync: updateEvaluationMutation.mutateAsync,
    deleteEvaluation: deleteEvaluationMutation.mutate,
    deleteEvaluationAsync: deleteEvaluationMutation.mutateAsync,

    // ------------------------------------------
    // Combined Loading / Saving Status
    // ------------------------------------------
    isSaving:
      updateHeaderMutation.isPending ||
      addIngredientMutation.isPending ||
      updateIngredientMutation.isPending ||
      deleteIngredientMutation.isPending ||
      updatePrepMethodMutation.isPending ||
      addEvaluationMutation.isPending ||
      updateEvaluationMutation.isPending ||
      deleteEvaluationMutation.isPending,
  };
};