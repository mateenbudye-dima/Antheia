using Antheia.Application.DTOs;
using Antheia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Application.Interfaces;

public interface IBlendService
{
    Task<DraftBlendCreatedDto> CreateDraftBlendAsync();
    Task<bool> UpdateHeaderAsync(int blendId, UpdateBlendHeaderDto dto);
    Task<int> AddSectionAsync(int blendId, AddSectionDto dto);
    Task DeleteSectionAsync(int sectionId);
    Task SyncIngredientsAsync(int sectionId, List<IngredientDto> ingredients);
    Task SavePrepMethodAsync(int sectionId, PrepMethodDto dto);
    Task SyncEvaluationsAsync(int sectionId, List<EvaluationDto> evaluations);
    Task<List<BlendListItemDto>> GetBlendListAsync();
    Task<GetBlendForEditDto?> GetBlendForEditAsync(int blendId);

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

    Task SubmitAsync(int entityId, BlendStatus submittedFor);
    Task ApproveAsync(int entityId, string? comments);
    Task RejectAsync(int entityId, string? comments);
}