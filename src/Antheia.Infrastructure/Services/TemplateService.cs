using Antheia.Application.DTOs;
using Antheia.Application.Interfaces;
using Antheia.Domain.Entities;
using Antheia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Antheia.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace Antheia.Infrastructure.Services;

public class TemplateService : ITemplateService
{
    private readonly AntheiaDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<TemplateService> _logger;

    public TemplateService(AntheiaDbContext context, ICurrentUserService currentUser, ILogger<TemplateService> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    // 1. Creates an initial draft template record upon page initialization
    public async Task<DraftTemplateCreatedDto> CreateDraftTemplateAsync()
    {
        _logger.LogInformation("CreateDraftTemplateAsync started by User:{UserId} Org:{OrgId}", _currentUser.UserId, _currentUser.OrganizationId);
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Create Base Template Record
            var template = new TemplateRecord
            {
                Title = "Untitled Template",
                OrganizationId = _currentUser.OrganizationId,
                AuthorId = _currentUser.UserId,
                CreatedBy = _currentUser.UserId,
                UpdatedBy = _currentUser.UserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsPublished = false,
                IsActive = true
            };

            _context.TemplateRecords.Add(template);
            await _context.SaveChangesAsync();
            _logger.LogDebug("CreateDraftTemplateAsync: base template created with temporary id {TempId}", template.TemplateId);

            // 2. Create Default "Ingredients" Section (SectionTypeId = 1)
            var ingredientsSection = new SectionRecord
            {
                ContainerId = template.TemplateId,
                ContainerTypeId = 1, // 1 = Template Container
                SectionTypeId = 1,
                SectionTitle = "Ingredients",
                SectionOrder = 1,
                IsActive = true,
                CreatedBy = _currentUser.UserId,
                UpdatedBy = _currentUser.UserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            _context.SectionRecords.Add(ingredientsSection);

            // 3. Create Default "Preparation Method" Section (SectionTypeId = 2)
            var prepSection = new SectionRecord
            {
                ContainerId = template.TemplateId,
                ContainerTypeId = 1,
                SectionTypeId = 2,
                SectionTitle = "Preparation Method",
                SectionOrder = 2,
                IsActive = true,
                CreatedBy = _currentUser.UserId,
                UpdatedBy = _currentUser.UserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            _context.SectionRecords.Add(prepSection);

            // 4. Create Default "Evaluation" Section (SectionTypeId = 3)
            var evalSection = new SectionRecord
            {
                ContainerId = template.TemplateId,
                ContainerTypeId = 1,
                SectionTypeId = 3,
                SectionTitle = "Emulsifier Blend Evaluation",
                SectionOrder = 3,
                IsActive = true,
                CreatedBy = _currentUser.UserId,
                UpdatedBy = _currentUser.UserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            _context.SectionRecords.Add(evalSection);

            await _context.SaveChangesAsync(); // Generates SectionIds

            // 5. Seed Default Evaluation Parameters
            var defaultEvaluationParams = new[]
            {
                "Appearance",
                "pH",
                "Solubility",
                "Compatibility",
                "Emulsion Test",
                "Hard Water Stability"
            };

            int paramTypeIndex = 1;
            foreach (var paramName in defaultEvaluationParams)
            {
                _context.Evaluations.Add(new Evaluation
                {
                    SectionId = evalSection.SectionId,
                    EvaluationParameterType = paramTypeIndex++,
                    Specification = paramName,
                    IsActive = true,
                    CreatedBy = _currentUser.UserId,
                    UpdatedBy = _currentUser.UserId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                });
            }

            // 6. Seed Default Preparation Method Container Record
            _context.PreparationMethods.Add(new PreparationMethod
            {
                SectionId = prepSection.SectionId,
                IsActive = true,
                CreatedBy = _currentUser.UserId,
                UpdatedBy = _currentUser.UserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("CreateDraftTemplateAsync: draft created TemplateId:{TemplateId}", template.TemplateId);

            return new DraftTemplateCreatedDto(
                template.TemplateId,
                ingredientsSection.SectionId,
                prepSection.SectionId,
                evalSection.SectionId
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "CreateDraftTemplateAsync failed and rolled back");
            throw;
        }
    }

    // 2. Patches title, objective, and description on blur or debounce
    public async Task<bool> UpdateHeaderAsync(int templateId, UpdateTemplateHeaderDto dto)
    {
        try
        {
            _logger.LogInformation("UpdateHeaderAsync called for TemplateId:{TemplateId} by User:{UserId}", templateId, _currentUser.UserId);
            var template = await _context.TemplateRecords
                .FirstOrDefaultAsync(t => t.TemplateId == templateId && t.IsActive);

            if (template == null)
            {
                _logger.LogWarning("UpdateHeaderAsync: template {TemplateId} not found", templateId);
                throw new NotFoundException($"Template with ID {templateId} not found or inactive.");
            }

            // Apply header updates
            template.Title = dto.Title;
            template.Objective = dto.Objective;
            template.Description = dto.Description;

            // Update audit fields
            template.UpdatedBy = _currentUser.UserId;
            template.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("UpdateHeaderAsync: template {TemplateId} updated", templateId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateHeaderAsync failed for TemplateId:{TemplateId}", templateId);
            throw;
        }
    }

    // 3. Creates a new section container
    public async Task<int> AddSectionAsync(int templateId, AddSectionDto dto)
    {
        try
        {
            var section = new SectionRecord
            {
            ContainerId = templateId,
            ContainerTypeId = 1, // 1 = Template Container
            SectionTypeId = dto.SectionTypeId,
            SectionTitle = dto.SectionTitle,
            SectionOrder = dto.SectionOrder,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            CreatedDate = DateTime.UtcNow,
            UpdatedBy = _currentUser.UserId,
            UpdatedDate = DateTime.UtcNow
            };

            _context.SectionRecords.Add(section);
            await _context.SaveChangesAsync();
            _logger.LogInformation("AddSectionAsync: created SectionId:{SectionId} for TemplateId:{TemplateId}", section.SectionId, templateId);
            return section.SectionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddSectionAsync failed for TemplateId:{TemplateId}", templateId);
            throw;
        }
    }

    // 4. Soft deletes a section
    public async Task DeleteSectionAsync(int sectionId)
    {
        var section = await _context.SectionRecords.FindAsync(sectionId);
        if (section == null) return;

        section.IsActive = false;
        section.UpdatedBy = _currentUser.UserId;
        section.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // 5. Reconciles and updates ingredients for a section
    public async Task SyncIngredientsAsync(int sectionId, List<IngredientDto> ingredients)
    {
        try
        {
            _logger.LogInformation("SyncIngredientsAsync called for SectionId:{SectionId} by User:{UserId}. Incoming count:{Count}", sectionId, _currentUser.UserId, ingredients?.Count ?? 0);
            var existing = await _context.Ingredients
                .Where(i => i.SectionId == sectionId)
                .ToListAsync();

            _context.Ingredients.RemoveRange(existing);

            var newEntries = (ingredients ?? Enumerable.Empty<IngredientDto>()).Select(i => new Ingredient
            {
                SectionId = sectionId,
                Name = i.Name,
                Type = i.Type,
                Ratio = i.Ratio,
                Quantity = i.Quantity,
                IsActive = true,
                CreatedBy = _currentUser.UserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = _currentUser.UserId,
                UpdatedDate = DateTime.UtcNow
            });

            _context.Ingredients.AddRange(newEntries);
            await _context.SaveChangesAsync();
            _logger.LogInformation("SyncIngredientsAsync: synced {Count} ingredients for SectionId:{SectionId}", ingredients?.Count ?? 0, sectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SyncIngredientsAsync failed for SectionId:{SectionId}", sectionId);
            throw;
        }
    }

    // 6. Upserts preparation method details
    public async Task SavePrepMethodAsync(int sectionId, PrepMethodDto dto)
    {
        try
        {
            _logger.LogInformation("SavePrepMethodAsync called for SectionId:{SectionId} by User:{UserId}", sectionId, _currentUser.UserId);
            var prep = await _context.PreparationMethods
                .FirstOrDefaultAsync(p => p.SectionId == sectionId);

            if (prep == null)
            {
                prep = new PreparationMethod
                {
                    SectionId = sectionId,
                    IsActive = true,
                    CreatedBy = _currentUser.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                _context.PreparationMethods.Add(prep);
            }

            prep.AdditionSequence = dto.AdditionSequence;
            prep.MixingSpeed = dto.MixingSpeed;
            prep.MixingTime = dto.MixingTime;
            prep.Temperature = dto.Temperature;
            prep.UpdatedBy = _currentUser.UserId;
            prep.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("SavePrepMethodAsync: preparation method upserted for SectionId:{SectionId}", sectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SavePrepMethodAsync failed for SectionId:{SectionId}", sectionId);
            throw;
        }
    }

    // 7. Reconciles evaluation parameters
    public async Task SyncEvaluationsAsync(int sectionId, List<EvaluationDto> evaluations)
    {
        try
        {
            _logger.LogInformation("SyncEvaluationsAsync called for SectionId:{SectionId} by User:{UserId}. Incoming count:{Count}", sectionId, _currentUser.UserId, evaluations?.Count ?? 0);
            var existing = await _context.Evaluations
                .Where(e => e.SectionId == sectionId)
                .ToListAsync();

            _context.Evaluations.RemoveRange(existing);

            var newEntries = (evaluations ?? Enumerable.Empty<EvaluationDto>()).Select(e => new Evaluation
            {
                SectionId = sectionId,
                EvaluationParameterType = e.EvaluationParameterType,
                Result = e.Result,
                Specification = e.Specification,
                Status = e.Status,
                IsActive = true,
                CreatedBy = _currentUser.UserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = _currentUser.UserId,
                UpdatedDate = DateTime.UtcNow
            });

            _context.Evaluations.AddRange(newEntries);
            await _context.SaveChangesAsync();
            _logger.LogInformation("SyncEvaluationsAsync: synced {Count} evaluations for SectionId:{SectionId}", evaluations?.Count ?? 0, sectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SyncEvaluationsAsync failed for SectionId:{SectionId}", sectionId);
            throw;
        }
    }

    public async Task<List<TemplateListItemDto>> GetTemplatesListAsync()
    {
        try
        {
            var list = await _context.TemplateRecords
                .AsNoTracking()
                .Where(t => t.OrganizationId == _currentUser.OrganizationId && t.IsActive)
                .OrderByDescending(t => t.UpdatedDate)
                .Select(t => new TemplateListItemDto(
                    t.TemplateId,
                    t.Title,
                    t.Objective,
                    t.UpdatedDate,
                    t.IsPublished
                ))
                .ToListAsync();

            _logger.LogInformation("GetTemplatesListAsync: returning {Count} templates for Org:{OrgId}", list.Count, _currentUser.OrganizationId);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTemplatesListAsync failed for Org:{OrgId}", _currentUser.OrganizationId);
            throw;
        }
    }

    //Get Template for Edit
    public async Task<GetTemplateForEditDto?> GetTemplateForEditAsync(int templateId)
    {
        try
        {
            _logger.LogInformation("GetTemplateForEditAsync called for TemplateId:{TemplateId} by User:{UserId}", templateId, _currentUser.UserId);

            var template = await _context.TemplateRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TemplateId == templateId && t.IsActive);

        if (template == null)
        {
            _logger.LogWarning("GetTemplateForEditAsync: template {TemplateId} not found", templateId);
            throw new NotFoundException($"Template with ID {templateId} not found.");
        }

            var sections = await _context.SectionRecords
                .AsNoTracking()
                .Where(s => s.ContainerId == templateId && s.ContainerTypeId == 1 && s.IsActive)
                .OrderBy(s => s.SectionOrder)
                .ToListAsync();

            var sectionIds = sections.Select(s => s.SectionId).ToList();

            var ingredients = await _context.Ingredients
                .AsNoTracking()
                .Where(i => sectionIds.Contains(i.SectionId) && i.IsActive)
                .ToListAsync();

            var prepMethods = await _context.PreparationMethods
                .AsNoTracking()
                .Where(p => sectionIds.Contains(p.SectionId) && p.IsActive)
                .ToListAsync();

            var evaluations = await _context.Evaluations
                .AsNoTracking()
                .Where(e => sectionIds.Contains(e.SectionId) && e.IsActive)
                .ToListAsync();

            var sectionDtos = sections.Select(s => new SectionEditDto(
                s.SectionId,
                s.SectionTypeId,
                s.SectionTitle,
                s.SectionOrder,
                ingredients.Where(i => i.SectionId == s.SectionId)
                           .Select(i => new IngredientEditDto(i.SectionIngredientId, i.Name, i.Type, i.Ratio, i.Quantity))
                           .ToList(),
                prepMethods.Where(p => p.SectionId == s.SectionId)
                           .Select(p => new PrepMethodEditDto(p.PreparationId, p.AdditionSequence, p.MixingSpeed, p.MixingTime, p.Temperature))
                           .FirstOrDefault(),
                evaluations.Where(e => e.SectionId == s.SectionId)
                           .Select(e => new EvaluationEditDto(e.EvaluationId, e.EvaluationParameterType, e.Result, e.Specification, e.Status))
                           .ToList()
            )).ToList();

            _logger.LogInformation("GetTemplateForEditAsync: template {TemplateId} retrieved with {SectionCount} sections", templateId, sectionDtos.Count);

            return new GetTemplateForEditDto(
                template.TemplateId,
                template.Title,
                template.Objective,
                template.Description,
                template.IsPublished,
                sectionDtos
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTemplateForEditAsync failed for TemplateId:{TemplateId}", templateId);
            throw;
        }
    }


    // --- Ingredients Implementation ---
    public async Task<IngredientResponseDto> AddIngredientAsync(CreateIngredientDto dto)
    {
        try
        {
            var ingredient = new Ingredient
            {
            SectionId = dto.SectionId,
            Name = dto.Name,
            Type = dto.Type,
            Ratio = dto.Ratio,
            Quantity = dto.Quantity,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            UpdatedBy = _currentUser.UserId,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
            };

            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            _logger.LogInformation("AddIngredientAsync: created IngredientId:{IngredientId} in SectionId:{SectionId}", ingredient.SectionIngredientId, ingredient.SectionId);

            return new IngredientResponseDto(
                ingredient.SectionIngredientId,
                ingredient.SectionId,
                ingredient.Name,
                ingredient.Type,
                ingredient.Ratio,
                ingredient.Quantity
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddIngredientAsync failed for SectionId:{SectionId}", dto.SectionId);
            throw;
        }
    }

    public async Task<bool> UpdateIngredientAsync(int ingredientId, UpdateIngredientDto dto)
    {
        try
        {
            _logger.LogInformation("UpdateIngredientAsync called for IngredientId:{IngredientId} by User:{UserId}", ingredientId, _currentUser.UserId);
            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(i => i.SectionIngredientId == ingredientId && i.IsActive);

            if (ingredient == null)
            {
                _logger.LogWarning("UpdateIngredientAsync: ingredient {IngredientId} not found", ingredientId);
                throw new NotFoundException($"Ingredient with ID {ingredientId} not found.");
            }

            ingredient.Name = dto.Name;
            ingredient.Type = dto.Type;
            ingredient.Ratio = dto.Ratio;
            ingredient.Quantity = dto.Quantity;
            ingredient.UpdatedBy = _currentUser.UserId;
            ingredient.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("UpdateIngredientAsync: ingredient {IngredientId} updated", ingredientId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateIngredientAsync failed for IngredientId:{IngredientId}", ingredientId);
            throw;
        }
    }

    public async Task<bool> DeleteIngredientAsync(int ingredientId)
    {
        try
        {
            _logger.LogInformation("DeleteIngredientAsync called for IngredientId:{IngredientId} by User:{UserId}", ingredientId, _currentUser.UserId);
            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(i => i.SectionIngredientId == ingredientId && i.IsActive);

            if (ingredient == null)
            {
                _logger.LogWarning("DeleteIngredientAsync: ingredient {IngredientId} not found", ingredientId);
                throw new NotFoundException($"Ingredient with ID {ingredientId} not found.");
            }

            // Soft delete
            ingredient.IsActive = false;
            ingredient.UpdatedBy = _currentUser.UserId;
            ingredient.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("DeleteIngredientAsync: ingredient {IngredientId} soft-deleted", ingredientId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteIngredientAsync failed for IngredientId:{IngredientId}", ingredientId);
            throw;
        }
    }

    // --- Preparation Method Implementation ---
    public async Task<PreparationMethodResponseDto?> UpdatePreparationMethodAsync(int prepId, UpdatePreparationMethodDto dto)
    {
        try
        {
            _logger.LogInformation("UpdatePreparationMethodAsync called for PrepId:{PrepId} by User:{UserId}", prepId, _currentUser.UserId);
            var prep = await _context.PreparationMethods
                .FirstOrDefaultAsync(p => p.PreparationId == prepId && p.IsActive);

            if (prep == null)
            {
                _logger.LogWarning("UpdatePreparationMethodAsync: preparation method {PrepId} not found", prepId);
                throw new NotFoundException($"Preparation method with ID {prepId} not found.");
            }

            prep.AdditionSequence = dto.AdditionSequence;
            prep.MixingSpeed = dto.MixingSpeed;
            prep.MixingTime = dto.MixingTime;
            prep.Temperature = dto.Temperature;
            prep.UpdatedBy = _currentUser.UserId;
            prep.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("UpdatePreparationMethodAsync: preparation method {PrepId} updated", prepId);

            return new PreparationMethodResponseDto(
                prep.PreparationId,
                prep.SectionId,
                prep.AdditionSequence,
                prep.MixingSpeed,
                prep.MixingTime,
                prep.Temperature
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdatePreparationMethodAsync failed for PrepId:{PrepId}", prepId);
            throw;
        }
    }

    // Evaluation Section Implementation ---
    public async Task<EvaluationResponseDto> AddEvaluationAsync(CreateEvaluationDto dto)
    {
        try
        {
            var evaluation = new Evaluation
            {
            SectionId = dto.SectionId,
            EvaluationParameterType = dto.EvaluationParameterType,
            Specification = dto.Specification,
            Result = dto.Result,
            Status = dto.Status,
            IsActive = true,
            CreatedBy = _currentUser.UserId,
            UpdatedBy = _currentUser.UserId,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
            };

            _context.Evaluations.Add(evaluation);
            await _context.SaveChangesAsync();
            _logger.LogInformation("AddEvaluationAsync: created EvaluationId:{EvaluationId} in SectionId:{SectionId}", evaluation.EvaluationId, evaluation.SectionId);

            return new EvaluationResponseDto(
                evaluation.EvaluationId,
                evaluation.SectionId,
                evaluation.EvaluationParameterType,
                evaluation.Specification,
                evaluation.Result,
                evaluation.Status
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddEvaluationAsync failed for SectionId:{SectionId}", dto.SectionId);
            throw;
        }
    }

    public async Task<bool> UpdateEvaluationAsync(int evaluationId, UpdateEvaluationDto dto)
    {
        try
        {
            _logger.LogInformation("UpdateEvaluationAsync called for EvaluationId:{EvaluationId} by User:{UserId}", evaluationId, _currentUser.UserId);
            var evaluation = await _context.Evaluations
                .FirstOrDefaultAsync(e => e.EvaluationId == evaluationId && e.IsActive);

            if (evaluation == null)
            {
                _logger.LogWarning("UpdateEvaluationAsync: evaluation {EvaluationId} not found", evaluationId);
                throw new NotFoundException($"Evaluation record with ID {evaluationId} not found.");
            }

            evaluation.Specification = dto.Specification;
            evaluation.Result = dto.Result;
            evaluation.Status = dto.Status;
            evaluation.UpdatedBy = _currentUser.UserId;
            evaluation.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("UpdateEvaluationAsync: evaluation {EvaluationId} updated", evaluationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateEvaluationAsync failed for EvaluationId:{EvaluationId}", evaluationId);
            throw;
        }
    }

    public async Task<bool> DeleteEvaluationAsync(int evaluationId)
    {
        try
        {
            _logger.LogInformation("DeleteEvaluationAsync called for EvaluationId:{EvaluationId} by User:{UserId}", evaluationId, _currentUser.UserId);
            var evaluation = await _context.Evaluations
                .FirstOrDefaultAsync(e => e.EvaluationId == evaluationId && e.IsActive);

            if (evaluation == null)
            {
                _logger.LogWarning("DeleteEvaluationAsync: evaluation {EvaluationId} not found", evaluationId);
                throw new NotFoundException($"Evaluation record with ID {evaluationId} not found.");
            }

            // Soft Delete
            evaluation.IsActive = false;
            evaluation.UpdatedBy = _currentUser.UserId;
            evaluation.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("DeleteEvaluationAsync: evaluation {EvaluationId} soft-deleted", evaluationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteEvaluationAsync failed for EvaluationId:{EvaluationId}", evaluationId);
            throw;
        }
    }
}