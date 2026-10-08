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
    Task<bool> DeleteSectionAsync(int sectionId);
    Task<PagedBlendListDto> GetBlendListAsync(
        string? search,
        BlendStatus[]? statuses,
        bool? isPublished,
        int page,
        int pageSize);
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

    Task<bool> SubmitAsync(int entityId, BlendStatus submittedFor);
    Task<bool> ApproveAsync(int entityId, string? comments, BlendStatus submittedFor);
    Task<bool> RejectAsync(int entityId, string? comments);
    Task<bool> CancelSubmissionAsync(int entityId, string? comments);
}