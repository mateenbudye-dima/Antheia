using Antheia.Application.DTOs;
using Antheia.Application.Exceptions;
using Antheia.Application.Interfaces;
using Antheia.Domain.Entities;
using Antheia.Domain.Enums;
using Antheia.Infrastructure.Data;
using Antheia.Infrastructure.Extensions;
using Dima.WorkFlowAuditMiddleware.Entities;
using Dima.WorkFlowAuditMiddleware.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Antheia.Infrastructure.Services;

public class BlendService : IBlendService
{
    private readonly AntheiaDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<BlendService> _logger;
    private readonly IWorkflowService _workflowService;

    public BlendService(AntheiaDbContext context, 
                            IWorkflowService workflowService,
                            ICurrentUserService currentUser, 
                            ILogger<BlendService> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
        _workflowService = workflowService;
    }

    // 1. Creates an initial draft blend record upon page initialization
    public async Task<DraftBlendCreatedDto> CreateDraftBlendAsync()
    {
        _logger.LogInformation("CreateDraftBlendAsync started by User:{UserId} Org:{OrgId}", _currentUser.UserId, _currentUser.OrganizationId);
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Create Base Blend Record
            var blend = new BlendRecord
            {
                Code = "",
                TrialNumber = null,
                Description = null,
                Objective = null,
                OrganizationId = _currentUser.OrganizationId,
                AuthorId = _currentUser.UserId,
                CreatedBy = _currentUser.UserId,
                UpdatedBy = _currentUser.UserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsPublished = false,
                IsActive = true
            };

            _context.BlendRecords.Add(blend);
            await _context.SaveChangesAsync();
            _logger.LogDebug("CreateDraftBlendAsync: base blend created with temporary id {TempId}", blend.BlendId);

            // 2. Create Default "Ingredients" Section (SectionTypeId = 1)
            var ingredientsSection = new SectionRecord
            {
                ContainerId = blend.BlendId,
                ContainerTypeId = SectionContainerType.Blend,
                SectionTypeId = SectionType.Ingredient,
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
                ContainerId = blend.BlendId,
                ContainerTypeId = SectionContainerType.Blend,
                SectionTypeId = SectionType.PreparationMethod,
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
                ContainerId = blend.BlendId,
                ContainerTypeId = SectionContainerType.Blend,
                SectionTypeId = SectionType.Evaluation,
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

            foreach (var paramName in defaultEvaluationParams)
            {
                _context.Evaluations.Add(new Evaluation
                {
                    SectionId = evalSection.SectionId,
                    EvaluationParameter = paramName,
                    Specification = string.Empty,
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

            _logger.LogInformation("CreateDraftBlendAsync: draft created BlendId:{BlendId}", blend.BlendId);

            return new DraftBlendCreatedDto(
                blend.BlendId,
                ingredientsSection.SectionId,
                prepSection.SectionId,
                evalSection.SectionId
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "CreateDraftBlendAsync failed and rolled back");
            throw;
        }
    }

    // 2. Patches title, objective, and description on blur or debounce
    public async Task<bool> UpdateHeaderAsync(int blendId, UpdateBlendHeaderDto dto)
    {
        try
        {
            _logger.LogInformation("UpdateHeaderAsync called for BlendId:{BlendId} by User:{UserId}", blendId, _currentUser.UserId);
            var blend = await _context.BlendRecords
                .FirstOrDefaultAsync(b => b.BlendId == blendId && b.IsActive);

            if (blend == null)
            {
                _logger.LogWarning("UpdateHeaderAsync: blend {BlendId} not found", blendId);
                throw new NotFoundException($"Blend with ID {blendId} not found or inactive.");
            }

            // Apply header updates
            blend.Code = dto.Code;
            blend.TrialNumber = dto.TrialNumber;
            blend.Objective = dto.Objective;
            blend.Description = dto.Description;

            // Update audit fields
            blend.UpdatedBy = _currentUser.UserId;
            blend.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("UpdateHeaderAsync: blend {BlendId} updated", blendId);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "UpdateHeaderAsync: database error for BlendId:{BlendId}", blendId);
            throw;
        }
    }

    // 3. Creates a new section container
    public async Task<int> AddSectionAsync(int blendId, AddSectionDto dto)
    {
        _logger.LogInformation("AddSectionAsync started for BlendId:{BlendId}, SectionType:{SectionType} by User:{UserId}",
            blendId, dto.SectionTypeId, _currentUser.UserId);

        // 1. Validate Blend Existence
        var blendExists = await _context.BlendRecords
            .AnyAsync(b => b.BlendId == blendId && b.OrganizationId == _currentUser.OrganizationId && b.IsActive);

        if (!blendExists)
        {
            throw new NotFoundException($"Blend with ID {blendId} was not found.");
        }
        EnsureParentStubsExist(blendId, dto.BlendCode);

        // 2. Prevent Duplicate Section Types
        var existingSectionTypes = await _context.SectionRecords
            .Where(s => s.ContainerId == blendId && s.ContainerTypeId == SectionContainerType.Blend && s.IsActive)
            .Select(s => s.SectionTypeId)
            .ToListAsync();

        //if (existingSectionTypes.Contains(dto.SectionType))
        //{
        //    throw new InvalidOperationException($"Section of type '{dto.SectionType}' already exists on this blend.");
        //}

        // Determine SectionOrder (put it after the last section)
        int maxOrder = await _context.SectionRecords
            .Where(s => s.ContainerId == blendId && s.ContainerTypeId == SectionContainerType.Blend && s.IsActive)
            .Select(s => (int?)s.SectionOrder)
            .MaxAsync() ?? 0;

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 3. Create Section Record based on SectionType
            var section = new SectionRecord
            {
                ContainerId = blendId,
                ContainerTypeId = SectionContainerType.Blend,
                SectionTypeId = dto.SectionTypeId,
                SectionTitle = GetSectionTitle(dto.SectionTypeId),
                SectionOrder = (byte)(maxOrder + 1),
                IsActive = true,
                CreatedBy = _currentUser.UserId,
                UpdatedBy = _currentUser.UserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.SectionRecords.Add(section);
            await _context.SaveChangesAsync(); // Generates SectionId

            // 4. Seed Specific Defaults Depending on Section Type
            switch (dto.SectionTypeId)
            {
                case SectionType.Ingredient:
                    // Ingredients start empty or ready for user input
                    break;

                case SectionType.PreparationMethod:
                    _context.PreparationMethods.Add(new PreparationMethod
                    {
                        SectionId = section.SectionId,
                        IsActive = true,
                        CreatedBy = _currentUser.UserId,
                        UpdatedBy = _currentUser.UserId,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    });
                    break;

                case SectionType.Evaluation:
                    var defaultParams = new[]
                    {
                    "Appearance",
                    "pH",
                    "Solubility",
                    "Compatibility",
                    "Emulsion Test",
                    "Hard Water Stability"
                    };

                    foreach (var paramName in defaultParams)
                    {
                        _context.Evaluations.Add(new Evaluation
                        {
                            SectionId = section.SectionId,
                            EvaluationParameter = paramName,
                            Specification = string.Empty,
                            IsActive = true,
                            CreatedBy = _currentUser.UserId,
                            UpdatedBy = _currentUser.UserId,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow
                        });
                    }
                    break;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("AddSectionAsync succeeded: SectionId:{SectionId} created for BlendId:{BlendId} (SectionType={SectionType})",
                section.SectionId, blendId, section.SectionTypeId);

            return section.SectionId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "AddSectionAsync failed for BlendId:{BlendId}", blendId);
            throw;
        }
    }
    
    // 4. Soft deletes a section
    public async Task<bool> DeleteSectionAsync(int sectionId)
    {
        _logger.LogInformation("DeleteSectionAsync called for SectionId:{SectionId} by User:{UserId}", sectionId, _currentUser.UserId);

        try
        {
            var section = await _context.SectionRecords.FindAsync(sectionId);
            if (section == null)
            {
                _logger.LogWarning("DeleteSectionAsync: section {SectionId} not found", sectionId);
                throw new NotFoundException($"Active section with ID {sectionId}");
            }

            section.IsActive = false;
            section.UpdatedBy = _currentUser.UserId;
            section.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("DeleteSectionAsync: section {SectionId} soft-deleted", sectionId);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "DeleteSectionAsync: database error while deleting SectionId:{SectionId}", sectionId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteSectionAsync failed for SectionId:{SectionId}", sectionId);
            throw;
        }
    }

    public async Task<List<BlendListItemDto>> GetBlendListAsync()
    {
        try
        {
            _logger.LogInformation("GetBlends called by {User}", _currentUser.UserId);
            var list = await _context.BlendRecords
                .AsNoTracking()
                .Where(b => b.OrganizationId == _currentUser.OrganizationId && b.IsActive)
                .OrderByDescending(b => b.UpdatedDate)
                .Select(b => new BlendListItemDto(
                    b.BlendId,
                    b.Code,
                    b.TrialNumber,
                    b.Objective,
                    b.UpdatedDate,
                    b.IsPublished,
                    b.Status?? BlendStatus.Draft,
                    b.CreatedBy
                ))
                .ToListAsync();

            _logger.LogInformation("GetBlendListAsync: returning {Count} blends for Org:{OrgId}", list.Count, _currentUser.OrganizationId);
            return list;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "GetBlendListAsync: database error for Org:{OrgId}", _currentUser.OrganizationId);
            throw;
        }
    }

    //Get Blend for Edit
    public async Task<GetBlendForEditDto?> GetBlendForEditAsync(int blendId)
    {
        try
        {
            _logger.LogInformation("GetBlendForEditAsync called for BlendId:{BlendId} by User:{UserId}", blendId, _currentUser.UserId);

            var blend = await _context.BlendRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BlendId == blendId && b.IsActive);

            if (blend == null)
            {
                _logger.LogWarning("GetBlendForEditAsync: blend {BlendId} not found", blendId);
                throw new NotFoundException($"Blend with ID {blendId} not found.");
            }

            var sections = await _context.SectionRecords
                .AsNoTracking()
                .Where(s => s.ContainerId == blendId && s.ContainerTypeId == SectionContainerType.Blend && s.IsActive)
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
                           .Select(e => new EvaluationEditDto(e.EvaluationId, e.EvaluationParameter, e.Result, e.Specification, e.Status))
                           .ToList()
            )).ToList();

            _logger.LogInformation("GetBlendForEditAsync: blend {BlendId} retrieved with {SectionCount} sections", blendId, sectionDtos.Count);

            return new GetBlendForEditDto(
                blend.BlendId,
                blend.Code,
                blend.TrialNumber,
                blend.Objective,
                blend.Description,
                blend.IsPublished,
                blend.Status?? BlendStatus.Draft,
                blend.CreatedBy,
                blend.UpdatedDate,
                sectionDtos
            )
            {

            };
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "GetBlendForEditAsync: database error for BlendId:{BlendId}", blendId);
            throw;
        }
    }


    // --- Ingredients Implementation ---
    public async Task<IngredientResponseDto> AddIngredientAsync(CreateIngredientDto dto)
    {
        if (dto == null)
        {
            _logger.LogWarning("AddIngredientAsync: incoming DTO is null");
            throw new ArgumentNullException(nameof(dto));
        }

        _logger.LogInformation("AddIngredientAsync called for SectionId:{SectionId} by User:{UserId}", dto.SectionId, _currentUser.UserId);

        EnsureParentStubsExist(dto.SectionId, dto.SectionTitle);
        
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
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "AddIngredientAsync: database error while adding ingredient to SectionId:{SectionId}", dto.SectionId);
            throw;
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

            EnsureParentStubsExist(dto.SectionId, dto.SectionTitle, dto.BlendId, dto.BlendCode);

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
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "UpdateIngredientAsync: database error for IngredientId:{IngredientId}", ingredientId);
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
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "DeleteIngredientAsync: database error for IngredientId:{IngredientId}", ingredientId);
            throw;
        }
    }

    // --- Preparation Method Implementation ---
    public async Task<PreparationMethodResponseDto?> UpdatePreparationMethodAsync(int prepId, UpdatePreparationMethodDto dto)
    {
        try
        {
            _logger.LogInformation("UpdatePreparationMethodAsync called for PrepId:{PrepId} by User:{UserId}", prepId, _currentUser.UserId);

            EnsureParentStubsExist(dto.SectionId, dto.SectionTitle, dto.BlendId, dto.BlendCode);

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
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "UpdatePreparationMethodAsync: database error for PrepId:{PrepId}", prepId);
            throw;
        }
    }

    // Evaluation Section Implementation ---
    public async Task<EvaluationResponseDto> AddEvaluationAsync(CreateEvaluationDto dto)
    {
        try
        {
            _logger.LogInformation("AddEvaluation called for SectionId={SectionId} by {User}", dto.SectionId, _currentUser.UserId);

            EnsureParentStubsExist(dto.SectionId, dto.SectionTitle, dto.BlendId, dto.BlendCode);

            var evaluation = new Evaluation
            {
            SectionId = dto.SectionId,
            EvaluationParameter = dto.EvaluationParameter,
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
                evaluation.EvaluationParameter,
                evaluation.Specification,
                evaluation.Result,
                evaluation.Status
            );
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "AddEvaluationAsync: database error for SectionId:{SectionId}", dto.SectionId);
            throw;
        }
    }

    public async Task<bool> UpdateEvaluationAsync(int evaluationId, UpdateEvaluationDto dto)
    {
        try
        {
            _logger.LogInformation("UpdateEvaluationAsync called for EvaluationId:{EvaluationId} by User:{UserId}", evaluationId, _currentUser.UserId);

            EnsureParentStubsExist(dto.SectionId, dto.SectionTitle, dto.BlendId, dto.BlendCode);

            var evaluation = await _context.Evaluations
                .FirstOrDefaultAsync(e => e.EvaluationId == evaluationId && e.IsActive);

            if (evaluation == null)
            {
                _logger.LogWarning("UpdateEvaluationAsync: evaluation {EvaluationId} not found", evaluationId);
                throw new NotFoundException($"Evaluation record with ID {evaluationId} not found.");
            }

            evaluation.EvaluationParameter = dto.EvaluationParameter;
            evaluation.Specification = dto.Specification;
            evaluation.Result = dto.Result;
            evaluation.Status = dto.Status;
            evaluation.UpdatedBy = _currentUser.UserId;
            evaluation.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("UpdateEvaluationAsync: evaluation {EvaluationId} updated", evaluationId);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "UpdateEvaluationAsync: database error for EvaluationId:{EvaluationId}", evaluationId);
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
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "DeleteEvaluationAsync: database error for EvaluationId:{EvaluationId}", evaluationId);
            throw;
        }
    }

    public async Task<bool> SubmitAsync(int blendId, BlendStatus submittedFor)
    {
        _logger.LogInformation("SubmitAsync called for BlendId:{BlendId} by User:{UserId}", blendId, _currentUser.UserId);
        try
        {
            var blend = await _context.BlendRecords.FirstOrDefaultAsync(b => b.BlendId == blendId && b.IsActive);
            if (blend == null)
            {
                _logger.LogWarning("SubmitAsync: blend {BlendId} not found", blendId);
                throw new NotFoundException($"Blend with ID {blendId} not found.");
            }

            blend.Status = submittedFor == BlendStatus.SubmittedForApproval ? BlendStatus.SubmittedForApproval : BlendStatus.SubmittedForReview;
            await _context.SaveChangesAsync();


            var auditFor = submittedFor == BlendStatus.SubmittedForApproval ? SubmittedFor.Approve : SubmittedFor.Review;
            await _workflowService.SubmitForApprovalAsync("Blend", blendId, _currentUser.UserId, auditFor);

            _logger.LogInformation("SubmitAsync: BlendId:{BlendId} submitted with status {Status}", blendId, blend.Status);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "SubmitAsync: DB error while submitting BlendId:{BlendId}", blendId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SubmitAsync failed for BlendId:{BlendId}", blendId);
            throw;
        }
    }
    public async Task<bool> ApproveAsync(int blendId, string? comments, BlendStatus submittedFor)
    {
        _logger.LogInformation("ApproveAsync called for BlendId:{BlendId} by User:{UserId}", blendId, _currentUser.UserId);
        try
        {
            var blend = await _context.BlendRecords.FirstOrDefaultAsync(b => b.BlendId == blendId && b.IsActive);
            if (blend == null)
            {
                _logger.LogWarning("ApproveAsync: blend {BlendId} not found", blendId);
                throw new NotFoundException($"Blend with ID {blendId} not found.");
            }

            blend.Status = submittedFor == BlendStatus.SubmittedForApproval ? BlendStatus.Approved : BlendStatus.Reviewed;
            await _context.SaveChangesAsync();

            var auditFor = submittedFor == BlendStatus.SubmittedForApproval ? SubmittedFor.Approve : SubmittedFor.Review;
            await _workflowService.ApproveAsync("Blend", blendId, _currentUser.UserId, auditFor, comments);

            _logger.LogInformation("ApproveAsync: BlendId:{BlendId} workflow approve invoked", blendId);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "ApproveAsync: DB error while approving BlendId:{BlendId}", blendId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApproveAsync failed for BlendId:{BlendId}", blendId);
            throw;
        }
    }
    public async Task<bool> RejectAsync(int blendId, string? comments)
    {
        _logger.LogInformation("RejectAsync called for BlendId:{BlendId} by User:{UserId}", blendId, _currentUser.UserId);
        try
        {
            var blend = await _context.BlendRecords.FirstOrDefaultAsync(b => b.BlendId == blendId && b.IsActive);
            if (blend == null)
            {
                _logger.LogWarning("RejectAsync: blend {BlendId} not found", blendId);
                throw new NotFoundException($"Blend with ID {blendId} not found.");
            }

            blend.Status = BlendStatus.Rejected;
            await _context.SaveChangesAsync();

            await _workflowService.RejectAsync("Blend", blendId, _currentUser.UserId, comments);

            _logger.LogInformation("RejectAsync: BlendId:{BlendId} workflow reject invoked", blendId);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "RejectAsync: DB error while rejecting BlendId:{BlendId}", blendId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RejectAsync failed for BlendId:{BlendId}", blendId);
            throw;
        }
    }
    public async Task<bool> CancelSubmissionAsync(int blendId, string? comments)
    {
        _logger.LogInformation("CancelSubmissionAsync called for BlendId:{BlendId} by User:{UserId}", blendId, _currentUser.UserId);
        try
        {
            var blend = await _context.BlendRecords.FirstOrDefaultAsync(b => b.BlendId == blendId && b.IsActive);
            if (blend == null)
            {
                _logger.LogWarning("CancelSubmissionAsync: blend {BlendId} not found", blendId);
                throw new NotFoundException($"Blend with ID {blendId} not found.");
            }

            blend.Status = BlendStatus.Draft;
            await _context.SaveChangesAsync();

            await _workflowService.CancelSubmissionAsync("Blend", blendId, _currentUser.UserId, comments);

            _logger.LogInformation("CancelSubmissionAsync: BlendId:{BlendId} workflow cancel submission invoked", blendId);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "CancelSubmissionAsync: DB error while cancelling submission BlendId:{BlendId}", blendId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CancelSubmissionAsync failed for BlendId:{BlendId}", blendId);
            throw;
        }
    }


    private static string GetSectionTitle(SectionType sectionType) => sectionType switch
    {
        SectionType.Ingredient => "Ingredients",
        SectionType.PreparationMethod => "Preparation Method",
        SectionType.Evaluation => "Emulsifier Blend Evaluation",
        _ => "Section"
    };
    private void EnsureParentStubsExist(int blendId, string? blendCode, int sectionId = 0, string? sectionTitle = null)
    {
        // 1. Stub Root Blend (Container) if ID provided
        if (blendId > 0)
        {
            _context.AttachStubIfMissing(
                b => b.BlendId == blendId,
                () => new BlendRecord
                {
                    BlendId = blendId,
                    Code = blendCode ?? $"Blend #{blendId}"
                });
        }

        // 2. Stub Parent Section if ID provided
        if (sectionId > 0)
        {
            _context.AttachStubIfMissing(
                s => s.SectionId == sectionId,
                () => new SectionRecord
                {
                    SectionId = sectionId,
                    SectionTitle = sectionTitle ?? string.Empty,
                    ContainerId = blendId,
                    ContainerTypeId = SectionContainerType.Blend
                });
        }
    }
}