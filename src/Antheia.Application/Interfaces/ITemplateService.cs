using Antheia.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Application.Interfaces;

public interface ITemplateService
{
    Task<int> CreateDraftTemplateAsync();
    Task UpdateHeaderAsync(int templateId, UpdateTemplateHeaderDto dto);
    Task<int> AddSectionAsync(int templateId, AddSectionDto dto);
    Task DeleteSectionAsync(int sectionId);
    Task SyncIngredientsAsync(int sectionId, List<IngredientDto> ingredients);
    Task SavePrepMethodAsync(int sectionId, PrepMethodDto dto);
    Task SyncEvaluationsAsync(int sectionId, List<EvaluationDto> evaluations);
}