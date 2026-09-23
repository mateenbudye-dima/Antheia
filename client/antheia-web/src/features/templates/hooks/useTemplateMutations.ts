// src/features/templates/hooks/useTemplateMutations.ts
import { useMutation, useQueryClient, useMutationState } from '@tanstack/react-query';
import {
  templatesApi,
  type UpdateTemplateHeaderDto,
  type CreateIngredientPayload,
  type UpdateIngredientPayload,
  type UpdatePrepMethodPayload,
  type CreateEvaluationPayload,
  type UpdateEvaluationPayload,
  type CreateSectionPayload,
} from '../api/templatesApi';
import type { SaveStatus } from '../../../shared/hooks/useAutoSave';

export const useTemplateMutations = (templateId: number) => {
  const queryClient = useQueryClient();
  const mutationKey = ['template-mutation', templateId];

  // 1. STABLE QUERY CACHE SUBSCRIPTION
  // Use pure primitives inside select so TanStack Query can memoize the output
  const mutationStates = useMutationState({
    filters: { mutationKey },
    select: (mutation) => ({
      status: mutation.state.status,
      submittedAt: mutation.state.submittedAt,
    }),
  });

  // 2. STABLE COMPUTATIONS (Without inline .sort() mutation)
  const isSaving = mutationStates.some((m) => m.status === 'pending');
  const hasError = mutationStates.some((m) => m.status === 'error');

  // Use Math.max or slice() before sorting to avoid inline array mutation
  const lastSubmittedAt = Math.max(0, ...mutationStates.map((m) => m.submittedAt));
  const lastMutation = mutationStates.find((m) => m.submittedAt === lastSubmittedAt);
  const isSuccess = lastMutation?.status === 'success';

  // Compute status
  let saveStatus: SaveStatus = 'idle';
  if (isSaving) {
    saveStatus = 'saving';
  } else if (hasError) {
    saveStatus = 'error';
  } else if (isSuccess) {
    saveStatus = 'saved';
  }

  // Centralized query cache invalidation helper
  const invalidateTemplate = () => {
    queryClient.invalidateQueries({ queryKey: ['template', templateId] });
    queryClient.invalidateQueries({ queryKey: ['templates'] });
  };

  // ==========================================
  // MUTATIONS (All using the same mutationKey)
  // ==========================================
  const updateHeaderMutation = useMutation({
    mutationKey,
    mutationFn: (payload: UpdateTemplateHeaderDto) =>
      templatesApi.updateTemplateHeader(templateId, payload),
    onSuccess: invalidateTemplate,
  });

  const addSectionMutation = useMutation({
    mutationKey,
    mutationFn: (payload: CreateSectionPayload) =>
      templatesApi.addSection(templateId, payload),
    onSuccess: invalidateTemplate,
  });

  // Delete Mutation
  const deleteSectionMutation = useMutation({
    mutationFn: (sectionId: number) => templatesApi.deleteSection(sectionId),
    onSuccess: () => {
      // Invalidate template query so UI, tree, and sections re-sync automatically
      queryClient.invalidateQueries({ queryKey: ['template', templateId] });
    },
  });

  const addIngredientMutation = useMutation({
    mutationKey,
    mutationFn: (payload: CreateIngredientPayload) =>
      templatesApi.addIngredient(payload),
    onSuccess: invalidateTemplate,
  });

  const updateIngredientMutation = useMutation({
    mutationKey,
    mutationFn: ({ id, payload }: { id: number; payload: UpdateIngredientPayload }) =>
      templatesApi.updateIngredient(id, payload),
    onSuccess: invalidateTemplate,
  });

  const deleteIngredientMutation = useMutation({
    mutationKey,
    mutationFn: (ingredientId: number) =>
      templatesApi.deleteIngredient(ingredientId),
    onSuccess: invalidateTemplate,
  });

  const updatePrepMethodMutation = useMutation({
    mutationKey,
    mutationFn: ({
      prepId,
      payload,
    }: {
      prepId: number;
      payload: UpdatePrepMethodPayload;
    }) => templatesApi.updatePrepMethod(prepId, payload),
    onSuccess: invalidateTemplate,
  });

  const addEvaluationMutation = useMutation({
    mutationKey,
    mutationFn: (payload: CreateEvaluationPayload) =>
      templatesApi.addEvaluation(payload),
    onSuccess: invalidateTemplate,
  });

  const updateEvaluationMutation = useMutation({
    mutationKey,
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
    mutationKey,
    mutationFn: (evaluationId: number) =>
      templatesApi.deleteEvaluation(evaluationId),
    onSuccess: invalidateTemplate,
  });

  return {
    // Header
    updateHeader: updateHeaderMutation.mutate,
    updateHeaderAsync: updateHeaderMutation.mutateAsync,

    //Section
    addSection: addSectionMutation.mutate,
    deleteSection: deleteSectionMutation.mutate,

    // Ingredients
    addIngredient: addIngredientMutation.mutate,
    addIngredientAsync: addIngredientMutation.mutateAsync,
    updateIngredient: updateIngredientMutation.mutate,
    updateIngredientAsync: updateIngredientMutation.mutateAsync,
    deleteIngredient: deleteIngredientMutation.mutate,
    deleteIngredientAsync: deleteIngredientMutation.mutateAsync,

    // Prep Methods
    updatePrepMethod: updatePrepMethodMutation.mutate,
    updatePrepMethodAsync: updatePrepMethodMutation.mutateAsync,

    // Evaluations
    addEvaluation: addEvaluationMutation.mutate,
    addEvaluationAsync: addEvaluationMutation.mutateAsync,
    updateEvaluation: updateEvaluationMutation.mutate,
    updateEvaluationAsync: updateEvaluationMutation.mutateAsync,
    deleteEvaluation: deleteEvaluationMutation.mutate,
    deleteEvaluationAsync: deleteEvaluationMutation.mutateAsync,

    // Status Indicators
    isSaving,
    saveStatus,
  };
};
