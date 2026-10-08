import type { BlendListFilters } from './blendsApi';

export const blendKeys = {
  all: ['blends'] as const,
  lists: (filters?: BlendListFilters) =>
    filters ? [...blendKeys.all, 'list', filters] as const : [...blendKeys.all, 'list'] as const,
  detail: (id: number) => [...blendKeys.all, 'detail', id] as const,

  // Mutation keys
  mutations: () => [...blendKeys.all, 'mutation'] as const,
  mutation: (id: number) => [...blendKeys.mutations(), id] as const,
};