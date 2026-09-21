using Antheia.Application.DTOs;
using Antheia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplatesController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplates()
    {
        var list = await _templateService.GetTemplatesListAsync();
        return Ok(list);
    }

    [HttpPost("draft")]
    public async Task<IActionResult> CreateDraft()
    {
        // No private claim parsing methods needed!
        var result = await _templateService.CreateDraftTemplateAsync();
        return Ok(result);
    }

    [HttpGet("{templateId}")]
    public async Task<IActionResult> GetTemplateForEdit(int templateId)
    {
        var template = await _templateService.GetTemplateForEditAsync(templateId);
        if (template == null) return NotFound();

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

        var success = await _templateService.UpdateHeaderAsync(templateId, dto);

        if (!success)
        {
            return NotFound(new { message = $"Template with ID {templateId} not found or inactive." });
        }

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

        var result = await _templateService.AddIngredientAsync(dto);
        return CreatedAtAction(nameof(GetTemplateForEdit), new { templateId = dto.SectionId }, result);
    }

    /// <summary>
    /// Updates an existing ingredient (debounced auto-save or inline edit).
    /// </summary>
    [HttpPatch("ingredients/{ingredientId:int}")]
    public async Task<IActionResult> UpdateIngredient(int ingredientId, [FromBody] UpdateIngredientDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _templateService.UpdateIngredientAsync(ingredientId, dto);
        if (!success) return NotFound(new { message = $"Ingredient with ID {ingredientId} not found." });

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

        var result = await _templateService.AddEvaluationAsync(dto);
        return CreatedAtAction(nameof(GetTemplateForEdit), new { templateId = dto.SectionId }, result);
    }

    /// <summary>
    /// Updates specification, result, or status for an evaluation parameter (debounced auto-save).
    /// </summary>
    [HttpPatch("evaluations/{evaluationId:int}")]
    public async Task<IActionResult> UpdateEvaluation(int evaluationId, [FromBody] UpdateEvaluationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _templateService.UpdateEvaluationAsync(evaluationId, dto);
        if (!success) return NotFound(new { message = $"Evaluation record with ID {evaluationId} not found." });

        return NoContent();
    }

    /// <summary>
    /// Soft deletes an evaluation parameter row.
    /// </summary>
    [HttpDelete("evaluations/{evaluationId:int}")]
    public async Task<IActionResult> DeleteEvaluation(int evaluationId)
    {
        var success = await _templateService.DeleteEvaluationAsync(evaluationId);
        if (!success) return NotFound(new { message = $"Evaluation record with ID {evaluationId} not found." });

        return NoContent();
    }
}