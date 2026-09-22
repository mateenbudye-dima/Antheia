import { useQuery } from '@tanstack/react-query';
import { templatesApi } from '../api/templatesApi';
import type { FullTemplateResponse } from '../types/template.types';

export const useTemplate = (id: string | undefined) => {
  const templateId = id ? parseInt(id, 10) : null;

  return useQuery<FullTemplateResponse, Error>({
    queryKey: ['template', templateId],
    queryFn: () => {
      if (!templateId || isNaN(templateId)) {
        throw new Error('Invalid template ID');
      }
      return templatesApi.getFullTemplate(templateId);
    },
    enabled: Boolean(templateId && !isNaN(templateId)),
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  });
};