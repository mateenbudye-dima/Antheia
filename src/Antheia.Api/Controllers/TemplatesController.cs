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

    [HttpPost("draft")]
    public async Task<IActionResult> CreateDraft()
    {
        // No private claim parsing methods needed!
        var templateId = await _templateService.CreateDraftTemplateAsync();
        return Ok(new { templateId });
    }
}