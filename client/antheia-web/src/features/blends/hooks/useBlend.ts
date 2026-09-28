import { useQuery } from '@tanstack/react-query';
import type { FullBlendResponse } from '../types/blend.types';
import { blendsApi } from '../api/blendsApi';

export const useBlend = (id: string | undefined) => {
  const blendId = id ? parseInt(id, 10) : null;

  return useQuery<FullBlendResponse, Error>({
    queryKey: ['blend', blendId],
    queryFn: () => {
      if (!blendId || isNaN(blendId)) {
        throw new Error('Invalid blend ID');
      }
      return blendsApi.getFullBlend(blendId);
    },
    enabled: Boolean(blendId && !isNaN(blendId)),
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  });
};