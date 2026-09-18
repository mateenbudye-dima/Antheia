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
    public async Task<int> CreateDraftTemplateAsync()
    {
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
        return template.TemplateId;
    }

    // 2. Patches title, objective, and description on blur or debounce
    public async Task UpdateHeaderAsync(int templateId, UpdateTemplateHeaderDto dto)
    {
        var template = await _context.TemplateRecords.FindAsync(templateId);
        if (template == null) return;

        template.Title = dto.Title;
        template.Objective = dto.Objective;
        template.Description = dto.Description;
        template.UpdatedBy = _currentUser.UserId;
        template.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
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
}