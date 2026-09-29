using Antheia.Application.DTOs;
using Antheia.Application.Interfaces;
using Antheia.Domain.Enums;
using Asp.Versioning;
using Dima.WorkFlowAuditMiddleware.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

[Authorize]
[ApiController]
[ApiVersion("1.0")] // Defines this controller as v1.0
[Route("api/v{version:apiVersion}/[controller]")]
public class BlendsController : ControllerBase
{
    private readonly IBlendService _blendService;
    private readonly ILogger<BlendsController> _logger;

    public BlendsController(IBlendService blendService, ILogger<BlendsController> logger)
    {
        _blendService = blendService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetBlends()
    {        
        var list = await _blendService.GetBlendListAsync();
        return Ok(list);
    }

    [HttpPost("draft")]
    public async Task<IActionResult> CreateDraft()
    {
        var result = await _blendService.CreateDraftBlendAsync();
        return Ok(result);
    }

    [HttpGet("{blendId}")]
    public async Task<IActionResult> GetBlendForEdit(int blendId)
    {
        var blend = await _blendService.GetBlendForEditAsync(blendId);
        return Ok(blend);
    }

    /// <summary>
    /// Updates blend header details (Title, Objective, Description).
    /// </summary>
    /// <param name="blendId">The target blend ID.</param>
    /// <param name="dto">Header update parameters.</param>
    [HttpPatch("{blendId:int}/header")]
    public async Task<IActionResult> UpdateHeader(int blendId, [FromBody] UpdateBlendHeaderDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _blendService.UpdateHeaderAsync(blendId, dto);
        return NoContent(); // 204 No Content for successful auto-save updates
    }

    /// <summary>
    /// Adds a new section to an existing blend draft.
    /// Endpoint: POST /api/blends/sections
    /// </summary>
    [HttpPost("{blendId:int}/sections")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSection(int blendId, [FromBody] AddSectionDto dto)
    {
        // 1. Basic validation
        if (dto == null || blendId <= 0)
        {
            _logger.LogWarning("AddSection: invalid parameters. BlendId={BlendId}, DTO null={IsDtoNull}", blendId, dto == null);
            return BadRequest(new { message = "Invalid blend parameters provided." });
        }

        // Enum range validation
        if (dto.SectionTypeId == SectionType.Unknown || !Enum.IsDefined(typeof(SectionType), dto.SectionTypeId))
        {
            _logger.LogWarning("AddSection: invalid SectionType provided for BlendId={BlendId}. SectionType={SectionType}", blendId, dto.SectionTypeId);
            return BadRequest(new { message = "Invalid SectionType provided." });
        }

        int newSectionId = await _blendService.AddSectionAsync(blendId, dto);

        return CreatedAtAction(
            nameof(GetBlendForEdit),
            new { blendId = blendId },
            new { sectionId = newSectionId, blendId, sectionType = dto.SectionTypeId }
        );
    }

    /// <summary>
    /// Deletes a specific section from a formulation blend draft.
    /// Endpoint: DELETE /api/blends/{blendId}/sections/{sectionId}
    /// </summary>
    [HttpDelete("{blendId:int}/sections/{sectionId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSection(int blendId, int sectionId)
    {
        if (sectionId <= 0)
        {
            _logger.LogWarning("DeleteSection: invalid sectionId provided. BlendId={BlendId}, SectionId={SectionId}", blendId, sectionId);
            return BadRequest(new { message = "Invalid blendId or sectionId provided." });
        }

        // Pass both if service verifies section belongs to blendId, otherwise just sectionId is enough
        var success = await _blendService.DeleteSectionAsync(sectionId);
        return Ok(new { message = "Section deleted successfully." });        
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
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("AddIngredient: invalid model state for incoming DTO. Errors={Errors}", ModelState.ErrorCount);
            return BadRequest(ModelState);
        }

        if (dto == null)
        {
            _logger.LogWarning("AddIngredient: DTO is null");
            return BadRequest(new { message = "Invalid ingredient data." });
        }

        var result = await _blendService.AddIngredientAsync(dto);
        _logger.LogInformation("AddIngredient created IngredientId={IngredientId} in SectionId={SectionId}", result?.SectionIngredientId, dto.SectionId);
        return CreatedAtAction(nameof(GetBlendForEdit), new { blendId = dto.SectionId }, result);
    }

    /// <summary>
    /// Updates an existing ingredient (debounced auto-save or inline edit).
    /// </summary>
    [HttpPatch("ingredients/{ingredientId:int}")]
    public async Task<IActionResult> UpdateIngredient(int ingredientId, [FromBody] UpdateIngredientDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var success = await _blendService.UpdateIngredientAsync(ingredientId, dto);      
        return NoContent();
    }

    /// <summary>
    /// Soft deletes an ingredient row from a section.
    /// </summary>
    [HttpDelete("ingredients/{ingredientId:int}")]
    public async Task<IActionResult> DeleteIngredient(int ingredientId)
    {
        var success = await _blendService.DeleteIngredientAsync(ingredientId);
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

        var result = await _blendService.UpdatePreparationMethodAsync(prepId, dto);
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
        var result = await _blendService.AddEvaluationAsync(dto);
        return CreatedAtAction(nameof(GetBlendForEdit), new { blendId = dto.SectionId }, result);
    }

    /// <summary>
    /// Updates specification, result, or status for an evaluation parameter (debounced auto-save).
    /// </summary>
    [HttpPatch("evaluations/{evaluationId:int}")]
    public async Task<IActionResult> UpdateEvaluation(int evaluationId, [FromBody] UpdateEvaluationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var success = await _blendService.UpdateEvaluationAsync(evaluationId, dto);        
        return NoContent();
    }

    /// <summary>
    /// Soft deletes an evaluation parameter row.
    /// </summary>
    [HttpDelete("evaluations/{evaluationId:int}")]
    public async Task<IActionResult> DeleteEvaluation(int evaluationId)
    {
        var success = await _blendService.DeleteEvaluationAsync(evaluationId);        
        return NoContent();
    }

    /// <summary>
    /// Submits a blend for approval. This action changes the blend's status to "SubmittedForApproval" and triggers the workflow audit process.
    /// </summary>
    /// <param name="id">blend id</param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("{id}/submitForApproval")]
    [AuditWorkflow("Blend", requiredApprovalRole: "BlendApprover")]
    public async Task<IActionResult> SubmitForApproval(int id)
    {
        var reviewerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        await _blendService.SubmitAsync(id, BlendStatus.SubmittedForApproval);
        return Ok(new { Message = $"Blend {id} submitted for approval successfully." });
    }

    /// <summary>
    /// Submits a blend for review. This action changes the blend's status to "SubmittedForReview" and triggers the workflow audit process.
    /// </summary>
    /// <param name="id">blend id</param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("{id}/submitForReview")]
    [AuditWorkflow("Blend", requiredApprovalRole: "BlendReviewer")]
    public async Task<IActionResult> SubmitForReview(int id)
    {
        var reviewerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        await _blendService.SubmitAsync(id, BlendStatus.SubmittedForReview);
        return Ok(new { Message = $"Blend {id} submitted for review successfully." });
    }

    /// <summary>
    /// Approves a submitted blend. This action changes the blend's status to "Approved" and triggers the workflow audit process.
    /// </summary>
    /// <param name="id">blend id</param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("{id}/approve")]
    [AuditWorkflow("Blend", requiredApprovalRole: "BlendApprover")]
    public async Task<IActionResult> Approve(int id, string comments)
    {
        var reviewerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        await _blendService.ApproveAsync(id, comments);
        return Ok(new { Message = $"Blend {id} approved successfully." });
    }
    /// <summary>
    /// Approves a submitted blend. This action changes the blend's status to "Approved" and triggers the workflow audit process.
    /// </summary>
    /// <param name="id">blend id</param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("{id}/review")]
    [AuditWorkflow("Blend", requiredApprovalRole: "BlendReviewer")]
    public async Task<IActionResult> Review(int id, string comments)
    {
        var reviewerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        await _blendService.ApproveAsync(id, comments);
        return Ok(new { Message = $"Blend {id} reviewed successfully." });
    }

    /// <summary>
    /// Rejects a submitted blend. This action changes the blend's status to "Rejected" and triggers the workflow audit process.
    /// </summary>
    /// <param name="id">blend id</param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("{id}/reject")]
    [AuditWorkflow("Blend", requiredApprovalRole: "BlendApprover")]
    public async Task<IActionResult> Reject(int id, string comments)
    {
        var reviewerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        await _blendService.RejectAsync(id, comments);
        return Ok(new { Message = $"Blend {id} rejected successfully." });
    }
}