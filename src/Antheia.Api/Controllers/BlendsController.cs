using Antheia.Application.DTOs;
using Antheia.Application.Interfaces;
using Antheia.Domain.Enums;
using Dima.WorkFlowAuditMiddleware.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Asp.Versioning;

namespace Antheia.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class BlendsController : ControllerBase
{
    private readonly IBlendService _blendService;
    private readonly ILogger<BlendsController> _logger;

    public BlendsController(IBlendService blendService, ILogger<BlendsController> logger)
    {
        _blendService = blendService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of all blends.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BlendListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBlends()
    {
        var list = await _blendService.GetBlendListAsync();
        return Ok(list);
    }

    /// <summary>
    /// Creates a new draft blend.
    /// </summary>
    [HttpPost("draft")]
    [ProducesResponseType(typeof(DraftBlendCreatedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDraft()
    {
        var result = await _blendService.CreateDraftBlendAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieves full details for editing a specific blend.
    /// </summary>
    /// <param name="blendId">The ID of the blend.</param>
    [HttpGet("{blendId:int}")]
    [ProducesResponseType(typeof(GetBlendForEditDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlendForEdit([FromRoute] int blendId)
    {
        var blend = await _blendService.GetBlendForEditAsync(blendId);
        if (blend == null)
        {
            return NotFound(new { message = $"Blend with ID {blendId} not found." });
        }
        return Ok(blend);
    }

    /// <summary>
    /// Updates blend header details (Title, Objective, Description).
    /// </summary>
    /// <param name="blendId">The target blend ID.</param>
    /// <param name="dto">Header update parameters.</param>
    [HttpPatch("{blendId:int}/header")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateHeader([FromRoute] int blendId, [FromBody] UpdateBlendHeaderDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _blendService.UpdateHeaderAsync(blendId, dto);
        return NoContent();
    }

    /// <summary>
    /// Adds a new section to an existing blend draft.
    /// </summary>
    /// <param name="blendId">The target blend ID.</param>
    /// <param name="dto">Section details.</param>
    [HttpPost("{blendId:int}/sections")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSection([FromRoute] int blendId, [FromBody] AddSectionDto dto)
    {
        if (dto == null || blendId <= 0)
        {
            _logger.LogWarning("AddSection: invalid parameters. BlendId={BlendId}, DTO null={IsDtoNull}", blendId, dto == null);
            return BadRequest(new { message = "Invalid blend parameters provided." });
        }

        if (dto.SectionTypeId == SectionType.Unknown || !Enum.IsDefined(typeof(SectionType), dto.SectionTypeId))
        {
            _logger.LogWarning("AddSection: invalid SectionType provided for BlendId={BlendId}. SectionType={SectionType}", blendId, dto.SectionTypeId);
            return BadRequest(new { message = "Invalid SectionType provided." });
        }

        int newSectionId = await _blendService.AddSectionAsync(blendId, dto);

        return CreatedAtAction(
            nameof(GetBlendForEdit),
            new { blendId },
            new { sectionId = newSectionId, blendId, sectionType = dto.SectionTypeId }
        );
    }

    /// <summary>
    /// Deletes a specific section from a formulation blend draft.
    /// </summary>
    /// <param name="blendId">The target blend ID.</param>
    /// <param name="sectionId">The target section ID.</param>
    [HttpDelete("{blendId:int}/sections/{sectionId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSection([FromRoute] int blendId, [FromRoute] int sectionId)
    {
        if (sectionId <= 0 || blendId <= 0)
        {
            _logger.LogWarning("DeleteSection: invalid parameters provided. BlendId={BlendId}, SectionId={SectionId}", blendId, sectionId);
            return BadRequest(new { message = "Invalid blendId or sectionId provided." });
        }

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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateIngredient([FromRoute] int ingredientId, [FromBody] UpdateIngredientDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _blendService.UpdateIngredientAsync(ingredientId, dto);
        return NoContent();
    }

    /// <summary>
    /// Soft deletes an ingredient row from a section.
    /// </summary>
    [HttpDelete("ingredients/{ingredientId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteIngredient([FromRoute] int ingredientId)
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePreparationMethod([FromRoute] int prepId, [FromBody] UpdatePreparationMethodDto dto)
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddEvaluation([FromBody] CreateEvaluationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _blendService.AddEvaluationAsync(dto);
        return CreatedAtAction(nameof(GetBlendForEdit), new { blendId = dto.SectionId }, result);
    }

    /// <summary>
    /// Updates specification, result, or status for an evaluation parameter.
    /// </summary>
    [HttpPatch("evaluations/{evaluationId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvaluation([FromRoute] int evaluationId, [FromBody] UpdateEvaluationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _blendService.UpdateEvaluationAsync(evaluationId, dto);
        return NoContent();
    }

    /// <summary>
    /// Soft deletes an evaluation parameter row.
    /// </summary>
    [HttpDelete("evaluations/{evaluationId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvaluation([FromRoute] int evaluationId)
    {
        var success = await _blendService.DeleteEvaluationAsync(evaluationId);
        return NoContent();
    }

    // ==========================================
    // WORKFLOW & APPROVAL ENDPOINTS
    // ==========================================

    /// <summary>
    /// Submits a blend for approval.
    /// </summary>
    /// <param name="id">The target blend ID.</param>
    [HttpPost("{id:int}/submit-for-approval")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitForApproval([FromRoute] int id)
    {
        var reviewerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        await _blendService.SubmitAsync(id, BlendStatus.SubmittedForApproval);
        return Ok(new { Message = $"Blend {id} submitted for approval successfully." });
    }

    /// <summary>
    /// Submits a blend for review.
    /// </summary>
    /// <param name="id">The target blend ID.</param>
    [HttpPost("{id:int}/submit-for-review")]
    [AuditWorkflow("Blend", requiredApprovalRole: "BlendReviewer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitForReview([FromRoute] int id)
    {
        var reviewerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        await _blendService.SubmitAsync(id, BlendStatus.SubmittedForReview);
        return Ok(new { Message = $"Blend {id} submitted for review successfully." });
    }

    /// <summary>
    /// Approves a submitted blend.
    /// </summary>
    /// <param name="id">The target blend ID.</param>
    /// <param name="comments">Approval comments.</param>
    [HttpPost("{id:int}/approve")]
    [AuditWorkflow("Blend", requiredApprovalRole: "BlendApprover")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve([FromRoute] int id, [FromBody] string comments)
    {
        var reviewerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        await _blendService.ApproveAsync(id, comments);
        return Ok(new { Message = $"Blend {id} approved successfully." });
    }

    /// <summary>
    /// Reviews a submitted blend.
    /// </summary>
    /// <param name="id">The target blend ID.</param>
    /// <param name="comments">Reviewer comments.</param>
    [HttpPost("{id:int}/review")]
    [AuditWorkflow("Blend", requiredApprovalRole: "BlendReviewer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Review([FromRoute] int id, [FromBody] string comments)
    {
        var reviewerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        await _blendService.ApproveAsync(id, comments);
        return Ok(new { Message = $"Blend {id} reviewed successfully." });
    }

    /// <summary>
    /// Rejects a submitted blend.
    /// </summary>
    /// <param name="id">The target blend ID.</param>
    /// <param name="comments">Rejection comments.</param>
    [HttpPost("{id:int}/reject")]
    [AuditWorkflow("Blend", requiredApprovalRole: "BlendApprover")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject([FromRoute] int id, [FromBody] string comments)
    {
        var reviewerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        await _blendService.RejectAsync(id, comments);
        return Ok(new { Message = $"Blend {id} rejected successfully." });
    }
}