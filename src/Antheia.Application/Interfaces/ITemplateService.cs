using Antheia.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Application.Interfaces;

public interface ITemplateService
{
    Task<DraftTemplateCreatedDto> CreateDraftTemplateAsync();
    Task<bool> UpdateHeaderAsync(int templateId, UpdateTemplateHeaderDto dto);
    Task<int> AddSectionAsync(int templateId, AddSectionDto dto);
    Task DeleteSectionAsync(int sectionId);
    Task SyncIngredientsAsync(int sectionId, List<IngredientDto> ingredients);
    Task SavePrepMethodAsync(int sectionId, PrepMethodDto dto);
    Task SyncEvaluationsAsync(int sectionId, List<EvaluationDto> evaluations);
    Task<List<TemplateListItemDto>> GetTemplatesListAsync();
    Task<GetTemplateForEditDto?> GetTemplateForEditAsync(int templateId);

    // Ingredients
    Task<IngredientResponseDto> AddIngredientAsync(CreateIngredientDto dto);
    Task<bool> UpdateIngredientAsync(int ingredientId, UpdateIngredientDto dto);
    Task<bool> DeleteIngredientAsync(int ingredientId);

    // Preparation Methods
    Task<PreparationMethodResponseDto?> UpdatePreparationMethodAsync(int prepId, UpdatePreparationMethodDto dto);

    // Evaluation Section
    Task<EvaluationResponseDto> AddEvaluationAsync(CreateEvaluationDto dto);
    Task<bool> UpdateEvaluationAsync(int evaluationId, UpdateEvaluationDto dto);
    Task<bool> DeleteEvaluationAsync(int evaluationId);
}