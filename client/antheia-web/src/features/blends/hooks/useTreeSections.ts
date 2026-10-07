import { useMemo } from 'react';
import { SectionType, type FullBlendResponse, type Section } from '../types/blend.types';
import { NodeType } from '../types/editor.types';
import type { SectionNode } from '../components/SectionTree';

const getSectionTypeLabel = (typeId: number): string => {
  switch (typeId) {
    case SectionType.Ingredients:
      return 'Ingredients';
    case SectionType.PreparationMethod:
      return 'Preparation Method';
    case SectionType.Evaluation:
      return 'Evaluation Parameters';
    default:
      return 'Section';
  }
};

export const useTreeSections = (data: FullBlendResponse | undefined): SectionNode[] => {
  return useMemo(() => {
    const nodes: SectionNode[] = [
      { id: 'header', title: 'Header & Overview', nodeType: NodeType.Header },
    ];

    if (!data?.sections || data.sections.length === 0) {
      return nodes;
    }

    // Calculate section type occurrences for numbering duplicates
    const typeCounts: Record<number, number> = {};
    data.sections.forEach((sec: Section) => {
      typeCounts[sec.sectionTypeId] = (typeCounts[sec.sectionTypeId] || 0) + 1;
    });

    const currentTypeIndex: Record<number, number> = {};

    data.sections.forEach((sec: Section) => {
      const baseTitle = sec.sectionTitle || getSectionTypeLabel(sec.sectionTypeId);
      const totalOfThisType = typeCounts[sec.sectionTypeId] || 0;

      let title = baseTitle;
      if (totalOfThisType > 1) {
        currentTypeIndex[sec.sectionTypeId] = (currentTypeIndex[sec.sectionTypeId] || 0) + 1;
        title = `${baseTitle} (${currentTypeIndex[sec.sectionTypeId]})`;
      }

      nodes.push({
        id: String(sec.sectionId),
        title,
        nodeType: sec.sectionTypeId,
      });
    });

    return nodes;
  }, [data]);
};