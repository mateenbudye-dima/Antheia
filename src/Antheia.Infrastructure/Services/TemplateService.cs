using Antheia.Application.DTOs;
using Antheia.Application.Interfaces;
using Antheia.Domain.Entities;
using Antheia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Antheia.Infrastructure.Services;

public class TemplateService : ITemplateService
{
    private readonly AntheiaDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public TemplateService(AntheiaDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    // 1. Creates an initial draft template record upon page initialization
    public async Task<DraftTemplateCreatedDto> CreateDraftTemplateAsync()
    {
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

            return new DraftTemplateCreatedDto(
                template.TemplateId,
                ingredientsSection.SectionId,
                prepSection.SectionId,
                evalSection.SectionId
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // 2. Patches title, objective, and description on blur or debounce
    public async Task<bool> UpdateHeaderAsync(int templateId, UpdateTemplateHeaderDto dto)
    {
        var template = await _context.TemplateRecords
            .FirstOrDefaultAsync(t => t.TemplateId == templateId && t.IsActive);

        if (template == null)
        {
            return false;
        }

        // Apply header updates
        template.Title = dto.Title;
        template.Objective = dto.Objective;
        template.Description = dto.Description;

        // Update audit fields
        template.UpdatedBy = _currentUser.UserId;
        template.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // 3. Creates a new section container
    public async Task<int> AddSectionAsync(int templateId, AddSectionDto dto)
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
        return section.SectionId;
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
        var existing = await _context.Ingredients
            .Where(i => i.SectionId == sectionId)
            .ToListAsync();

        _context.Ingredients.RemoveRange(existing);

        var newEntries = ingredients.Select(i => new Ingredient
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
    }

    // 6. Upserts preparation method details
    public async Task SavePrepMethodAsync(int sectionId, PrepMethodDto dto)
    {
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
    }

    // 7. Reconciles evaluation parameters
    public async Task SyncEvaluationsAsync(int sectionId, List<EvaluationDto> evaluations)
    {
        var existing = await _context.Evaluations
            .Where(e => e.SectionId == sectionId)
            .ToListAsync();

        _context.Evaluations.RemoveRange(existing);

        var newEntries = evaluations.Select(e => new Evaluation
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
    }

    public async Task<List<TemplateListItemDto>> GetTemplatesListAsync()
    {
        return await _context.TemplateRecords
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
    }

    //Get Template for Edit
    public async Task<GetTemplateForEditDto?> GetTemplateForEditAsync(int templateId)
    {
        var template = await _context.TemplateRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TemplateId == templateId && t.IsActive);

        if (template == null) return null;

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

        return new GetTemplateForEditDto(
            template.TemplateId,
            template.Title,
            template.Objective,
            template.Description,
            template.IsPublished,
            sectionDtos
        );
    }


    // --- Ingredients Implementation ---
    public async Task<IngredientResponseDto> AddIngredientAsync(CreateIngredientDto dto)
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

        return new IngredientResponseDto(
            ingredient.SectionIngredientId,
            ingredient.SectionId,
            ingredient.Name,
            ingredient.Type,
            ingredient.Ratio,
            ingredient.Quantity
        );
    }

    public async Task<bool> UpdateIngredientAsync(int ingredientId, UpdateIngredientDto dto)
    {
        var ingredient = await _context.Ingredients
            .FirstOrDefaultAsync(i => i.SectionIngredientId == ingredientId && i.IsActive);

        if (ingredient == null) return false;

        ingredient.Name = dto.Name;
        ingredient.Type = dto.Type;
        ingredient.Ratio = dto.Ratio;
        ingredient.Quantity = dto.Quantity;
        ingredient.UpdatedBy = _currentUser.UserId;
        ingredient.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteIngredientAsync(int ingredientId)
    {
        var ingredient = await _context.Ingredients
            .FirstOrDefaultAsync(i => i.SectionIngredientId == ingredientId && i.IsActive);

        if (ingredient == null) return false;

        // Soft delete
        ingredient.IsActive = false;
        ingredient.UpdatedBy = _currentUser.UserId;
        ingredient.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // --- Preparation Method Implementation ---
    public async Task<PreparationMethodResponseDto?> UpdatePreparationMethodAsync(int prepId, UpdatePreparationMethodDto dto)
    {
        var prep = await _context.PreparationMethods
            .FirstOrDefaultAsync(p => p.PreparationId == prepId && p.IsActive);

        if (prep == null) return null;

        prep.AdditionSequence = dto.AdditionSequence;
        prep.MixingSpeed = dto.MixingSpeed;
        prep.MixingTime = dto.MixingTime;
        prep.Temperature = dto.Temperature;
        prep.UpdatedBy = _currentUser.UserId;
        prep.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new PreparationMethodResponseDto(
            prep.PreparationId,
            prep.SectionId,
            prep.AdditionSequence,
            prep.MixingSpeed,
            prep.MixingTime,
            prep.Temperature
        );
    }

    // Evaluation Section Implementation ---
    public async Task<EvaluationResponseDto> AddEvaluationAsync(CreateEvaluationDto dto)
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

        return new EvaluationResponseDto(
            evaluation.EvaluationId,
            evaluation.SectionId,
            evaluation.EvaluationParameterType,
            evaluation.Specification,
            evaluation.Result,
            evaluation.Status
        );
    }

    public async Task<bool> UpdateEvaluationAsync(int evaluationId, UpdateEvaluationDto dto)
    {
        var evaluation = await _context.Evaluations
            .FirstOrDefaultAsync(e => e.EvaluationId == evaluationId && e.IsActive);

        if (evaluation == null) return false;

        evaluation.Specification = dto.Specification;
        evaluation.Result = dto.Result;
        evaluation.Status = dto.Status;
        evaluation.UpdatedBy = _currentUser.UserId;
        evaluation.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteEvaluationAsync(int evaluationId)
    {
        var evaluation = await _context.Evaluations
            .FirstOrDefaultAsync(e => e.EvaluationId == evaluationId && e.IsActive);

        if (evaluation == null) return false;

        // Soft Delete
        evaluation.IsActive = false;
        evaluation.UpdatedBy = _currentUser.UserId;
        evaluation.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}