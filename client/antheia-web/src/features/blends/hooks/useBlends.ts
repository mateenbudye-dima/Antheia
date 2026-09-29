// features/blends/hooks/useBlends.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { blendsApi } from '../api/blendsApi';
import { blendKeys } from '../api/blendKeys';

// Hook 1: Fetch list of blends
export const useBlends = () => {
  return useQuery({
    queryKey: blendKeys.lists(),
    queryFn: blendsApi.getBlends,
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  });
};

// Hook 2: Create a new draft blend
export const useCreateBlendDraft = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: blendsApi.createDraft,
    onSuccess: () => {
      // Invalidate the blends list query key
      queryClient.invalidateQueries({ queryKey: blendKeys.lists() });
    },
  });
};