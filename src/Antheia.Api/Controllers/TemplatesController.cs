using Antheia.Application.DTOs;
using Antheia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(ITemplateService templateService, ILogger<TemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplates()
    {
        _logger.LogInformation("GetTemplates called by {User}", User?.Identity?.Name ?? "anonymous");
        var list = await _templateService.GetTemplatesListAsync();
        _logger.LogInformation("GetTemplates returned {Count} templates", list?.Count ?? 0);
        return Ok(list);
    }

    [HttpPost("draft")]
    public async Task<IActionResult> CreateDraft()
    {
        _logger.LogInformation("CreateDraft called by {User}", User?.Identity?.Name ?? "anonymous");
        var result = await _templateService.CreateDraftTemplateAsync();
        _logger.LogInformation("CreateDraft created template {TemplateId}", result?.TemplateId);
        return Ok(result);
    }

    [HttpGet("{templateId}")]
    public async Task<IActionResult> GetTemplateForEdit(int templateId)
    {
        _logger.LogInformation("GetTemplateForEdit called for TemplateId={TemplateId} by {User}", templateId, User?.Identity?.Name ?? "anonymous");
        var template = await _templateService.GetTemplateForEditAsync(templateId);
        if (template == null)
        {
            _logger.LogWarning("GetTemplateForEdit: template {TemplateId} not found", templateId);
            return NotFound();
        }

        _logger.LogInformation("GetTemplateForEdit: template {TemplateId} retrieved", templateId);
        return Ok(template);
    }

    /// <summary>
    /// Updates template header details (Title, Objective, Description).
    /// </summary>
    /// <param name="templateId">The target template ID.</param>
    /// <param name="dto">Header update parameters.</param>
    [HttpPatch("{templateId:int}/header")]
    public async Task<IActionResult> UpdateHeader(int templateId, [FromBody] UpdateTemplateHeaderDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _logger.LogInformation("UpdateHeader called for TemplateId={TemplateId} by {User}", templateId, User?.Identity?.Name ?? "anonymous");

        var success = await _templateService.UpdateHeaderAsync(templateId, dto);

        if (!success)
        {
            _logger.LogWarning("UpdateHeader: template {TemplateId} not found or inactive", templateId);
            return NotFound(new { message = $"Template with ID {templateId} not found or inactive." });
        }

        _logger.LogInformation("UpdateHeader: template {TemplateId} updated", templateId);
        return NoContent(); // 204 No Content for successful auto-save updates
    }

    // ==========================================
    // INGREDIENTS ENDPOINTS
    // ==========================================

    /// <summary>
    /// Adds a new ingredient row to an Ingredients section.
    /// </summary>
    [HttpPost("ingredients")]
    public async Task<IActionResult> AddIngredient([FromBody] CreateIngredientDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        _logger.LogInformation("AddIngredient called for SectionId={SectionId} by {User}", dto.SectionId, User?.Identity?.Name ?? "anonymous");

        var result = await _templateService.AddIngredientAsync(dto);
        _logger.LogInformation("AddIngredient created IngredientId={IngredientId} in SectionId={SectionId}", result?.SectionIngredientId, dto.SectionId);
        return CreatedAtAction(nameof(GetTemplateForEdit), new { templateId = dto.SectionId }, result);
    }

    /// <summary>
    /// Updates an existing ingredient (debounced auto-save or inline edit).
    /// </summary>
    [HttpPatch("ingredients/{ingredientId:int}")]
    public async Task<IActionResult> UpdateIngredient(int ingredientId, [FromBody] UpdateIngredientDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        _logger.LogInformation("UpdateIngredient called for IngredientId={IngredientId} by {User}", ingredientId, User?.Identity?.Name ?? "anonymous");

        var success = await _templateService.UpdateIngredientAsync(ingredientId, dto);
        if (!success)
        {
            _logger.LogWarning("UpdateIngredient: ingredient {IngredientId} not found", ingredientId);
            return NotFound(new { message = $"Ingredient with ID {ingredientId} not found." });
        }

        _logger.LogInformation("UpdateIngredient: ingredient {IngredientId} updated", ingredientId);
        return NoContent();
    }

    /// <summary>
    /// Soft deletes an ingredient row from a section.
    /// </summary>
    [HttpDelete("ingredients/{ingredientId:int}")]
    public async Task<IActionResult> DeleteIngredient(int ingredientId)
    {
        var success = await _templateService.DeleteIngredientAsync(ingredientId);
        if (!success) return NotFound(new { message = $"Ingredient with ID {ingredientId} not found." });

        return NoContent();
    }

    // ==========================================
    // PREPARATION METHOD ENDPOINTS
    // ==========================================

    /// <summary>
    /// Updates the preparation parameters for a section (debounced auto-save).
    /// </summary>
    [HttpPatch("prep-methods/{prepId:int}")]
    public async Task<IActionResult> UpdatePreparationMethod(int prepId, [FromBody] UpdatePreparationMethodDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _templateService.UpdatePreparationMethodAsync(prepId, dto);
        if (result == null) return NotFound(new { message = $"Preparation method record with ID {prepId} not found." });

        return Ok(result);
    }


    // ==========================================
    // EVALUATION SECTION ENDPOINTS
    // ==========================================

    /// <summary>
    /// Adds a new custom evaluation parameter row to an Evaluation section.
    /// </summary>
    [HttpPost("evaluations")]
    public async Task<IActionResult> AddEvaluation([FromBody] CreateEvaluationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        _logger.LogInformation("AddEvaluation called for SectionId={SectionId} by {User}", dto.SectionId, User?.Identity?.Name ?? "anonymous");

        var result = await _templateService.AddEvaluationAsync(dto);
        _logger.LogInformation("AddEvaluation created EvaluationId={EvaluationId} in SectionId={SectionId}", result?.EvaluationId, dto.SectionId);
        return CreatedAtAction(nameof(GetTemplateForEdit), new { templateId = dto.SectionId }, result);
    }

    /// <summary>
    /// Updates specification, result, or status for an evaluation parameter (debounced auto-save).
    /// </summary>
    [HttpPatch("evaluations/{evaluationId:int}")]
    public async Task<IActionResult> UpdateEvaluation(int evaluationId, [FromBody] UpdateEvaluationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        _logger.LogInformation("UpdateEvaluation called for EvaluationId={EvaluationId} by {User}", evaluationId, User?.Identity?.Name ?? "anonymous");

        var success = await _templateService.UpdateEvaluationAsync(evaluationId, dto);
        if (!success)
        {
            _logger.LogWarning("UpdateEvaluation: evaluation {EvaluationId} not found", evaluationId);
            return NotFound(new { message = $"Evaluation record with ID {evaluationId} not found." });
        }

        _logger.LogInformation("UpdateEvaluation: evaluation {EvaluationId} updated", evaluationId);
        return NoContent();
    }

    /// <summary>
    /// Soft deletes an evaluation parameter row.
    /// </summary>
    [HttpDelete("evaluations/{evaluationId:int}")]
    public async Task<IActionResult> DeleteEvaluation(int evaluationId)
    {
        _logger.LogInformation("DeleteEvaluation called for EvaluationId={EvaluationId} by {User}", evaluationId, User?.Identity?.Name ?? "anonymous");

        var success = await _templateService.DeleteEvaluationAsync(evaluationId);
        if (!success)
        {
            _logger.LogWarning("DeleteEvaluation: evaluation {EvaluationId} not found", evaluationId);
            return NotFound(new { message = $"Evaluation record with ID {evaluationId} not found." });
        }

        _logger.LogInformation("DeleteEvaluation: evaluation {EvaluationId} soft-deleted", evaluationId);
        return NoContent();
    }
}