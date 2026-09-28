import { useMutation, useQueryClient, useMutationState } from '@tanstack/react-query';
  import {
    blendsApi,
  type UpdateBlendHeaderDto,
  type CreateIngredientPayload,
  type UpdateIngredientPayload,
  type UpdatePrepMethodPayload,
  type CreateEvaluationPayload,
  type UpdateEvaluationPayload,
  type CreateSectionPayload,
} from '../api/blendsApi';
import type { SaveStatus } from '../../../shared/hooks/useAutoSave';

export const useBlendMutations = (blendId: number) => {
  const queryClient = useQueryClient();
  const mutationKey = ['blend-mutation', blendId];

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
  const invalidateBlend = () => {
    queryClient.invalidateQueries({ queryKey: ['blend', blendId] });
    queryClient.invalidateQueries({ queryKey: ['blends'] });
  };

  // ==========================================
  // MUTATIONS (All using the same mutationKey)
  // ==========================================
  const updateHeaderMutation = useMutation({
    mutationKey,
    mutationFn: (payload: UpdateBlendHeaderDto) =>
      blendsApi.updateBlendHeader(blendId, payload),
    onSuccess: invalidateBlend,
  });

  const addSectionMutation = useMutation({
    mutationKey,
    mutationFn: (payload: CreateSectionPayload) =>
      blendsApi.addSection(blendId, payload),
    onSuccess: invalidateBlend,
  });

  // Delete Mutation
  const deleteSectionMutation = useMutation({
    mutationFn: (sectionId: number) => blendsApi.deleteSection(sectionId),
    onSuccess: () => {
      // Invalidate blend query so UI, tree, and sections re-sync automatically
      queryClient.invalidateQueries({ queryKey: ['blend', blendId] });
    },
  });

  const addIngredientMutation = useMutation({
    mutationKey,
    mutationFn: (payload: CreateIngredientPayload) =>
      blendsApi.addIngredient(payload),
    onSuccess: invalidateBlend,
  });

  const updateIngredientMutation = useMutation({
    mutationKey,
    mutationFn: ({ id, payload }: { id: number; payload: UpdateIngredientPayload }) =>
      blendsApi.updateIngredient(id, payload),
    onSuccess: invalidateBlend,
  });

  const deleteIngredientMutation = useMutation({
    mutationKey,
    mutationFn: (ingredientId: number) =>
      blendsApi.deleteIngredient(ingredientId),
    onSuccess: invalidateBlend,
  });

  const updatePrepMethodMutation = useMutation({
    mutationKey,
    mutationFn: ({
      prepId,
      payload,
    }: {
      prepId: number;
      payload: UpdatePrepMethodPayload;
    }) => blendsApi.updatePrepMethod(prepId, payload),
    onSuccess: invalidateBlend,
  });

  const addEvaluationMutation = useMutation({
    mutationKey,
    mutationFn: (payload: CreateEvaluationPayload) =>
      blendsApi.addEvaluation(payload),
    onSuccess: invalidateBlend,
  });

  const updateEvaluationMutation = useMutation({
    mutationKey,
    mutationFn: ({
      evaluationId,
      payload,
    }: {
      evaluationId: number;
      payload: UpdateEvaluationPayload;
    }) => blendsApi.updateEvaluation(evaluationId, payload),
    onSuccess: invalidateBlend,
  });

  const deleteEvaluationMutation = useMutation({
    mutationKey,
    mutationFn: (evaluationId: number) =>
      blendsApi.deleteEvaluation(evaluationId),
    onSuccess: invalidateBlend,
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
