import React, { useState } from 'react';
import type { EvaluationItem } from '../types/blend.types';
import { EvaluationRow } from './EvaluationRow';
import { useBlendMutations } from '../hooks/useBlendMutations';
import { BlendSectionTable, type ColumnConfig } from './shared/BlendSectionTable';

const COLUMNS: ColumnConfig[] = [
  { label: 'Parameter', width: '35%' },
  { label: 'Result', width: '25%' },
  { label: 'Specification', width: '25%' },
  { label: 'Status', width: '10%' },
  { label: 'Actions', width: '5%', align: 'center' },
];

interface Props {
  blendId: number;
  sectionId: number;
  sectionTitle: string;
  blendCode: string;
  initialEvaluations: EvaluationItem[];
  readOnly?: boolean;
}

export const EvaluationSection: React.FC<Props> = ({
  blendId,
  sectionId,
  initialEvaluations,
  sectionTitle,
  blendCode,
  readOnly = false,
}) => {
  const [evaluations, setEvaluations] = useState<EvaluationItem[]>(initialEvaluations);
  const { addEvaluationAsync, deleteEvaluationAsync } = useBlendMutations(blendId);

  const addParam = async () => {
    try {
      const newEvaluation = await addEvaluationAsync({
        sectionId,
        evaluationParameter: 'Parameter Name',
        specification: '',
        result: '',
        status: 'Pending',
        sectionTitle: sectionTitle,
        blendId: blendId,
        blendCode: blendCode,  
      });
      setEvaluations((prev) => [...prev, newEvaluation]);
    } catch (err) {
      console.error('Failed adding evaluation:', err);
    }
  };

  const updateField = (id: number, field: keyof EvaluationItem, value: string) => {
    setEvaluations((prev) =>
      prev.map((item) => (item.evaluationId === id ? { ...item, [field]: value } : item))
    );
  };

  const deleteRow = async (id: number) => {
    try {
      await deleteEvaluationAsync(id);
      setEvaluations((prev) => prev.filter((item) => item.evaluationId !== id));
    } catch (err) {
      console.error('Failed deleting evaluation row:', err);
    }
  };

  return (
    <BlendSectionTable
      columns={COLUMNS}
      addButtonLabel="Add Evaluation Parameter"
      onAddRow={addParam}
      readOnly={readOnly}
    >
      {evaluations.map((item) => (
        <EvaluationRow
          key={item.evaluationId}
          blendId={blendId}
          blendCode={blendCode}
          sectionId={sectionId}
          sectionTitle={sectionTitle}
          item={item}
          onChange={updateField}
          onDelete={deleteRow}
          readOnly={readOnly}
        />
      ))}
    </BlendSectionTable>
  );
};