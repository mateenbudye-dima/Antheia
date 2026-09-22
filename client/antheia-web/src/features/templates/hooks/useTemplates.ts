import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import apiClient from '../../../shared/api/apiClient';

export interface TemplateItem {
  templateId: number;
  title: string;
  objective: string | null;
  updatedDate: string;
  isPublished: boolean | null;
}

// Hook 1: Fetch list of templates
export const useTemplates = () => {
  return useQuery({
    queryKey: ['templates'],
    queryFn: async (): Promise<TemplateItem[]> => {
      const response = await apiClient.get<TemplateItem[]>('/templates');
      return response.data;
    },
  });
};

// Hook 2: Create a new draft template
export const useCreateTemplateDraft = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (): Promise<{ templateId: number }> => {
      const response = await apiClient.post<{ templateId: number }>('/templates/draft');
      return response.data;
    },
    onSuccess: () => {
      // Invalidate the templates list cache so returning back displays the new item
      queryClient.invalidateQueries({ queryKey: ['templates'] });
    },
  });
};