import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../../shared/api/apiClient';

export interface BlendItem {
  blendId: number;
  code: string;
  objective: string | null;
  updatedDate: string;
  isPublished: boolean | null;
}

// Hook 1: Fetch list of blends
export const useBlends = () => {
  return useQuery({
    queryKey: ['blends'],
    queryFn: async (): Promise<BlendItem[]> => {
      const response = await apiClient.get<BlendItem[]>('/blends');
      return response.data;
    },
  });
};

// Hook 2: Create a new draft blend
export const useCreateBlendDraft = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (): Promise<{ blendId: number }> => {
      const response = await apiClient.post<{ blendId: number }>('/blends/draft');
      return response.data;
    },
    onSuccess: () => {
      // Invalidate the blends list cache so returning back displays the new item
      queryClient.invalidateQueries({ queryKey: ['blends'] });
    },
  });
};