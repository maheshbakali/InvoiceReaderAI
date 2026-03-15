using InvoiceAI.Models;
using InvoiceAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceAI.Controllers;

/// <summary>
/// Invoice extraction endpoint – accepts raw invoice text and returns
/// structured JSON with all extracted fields.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceExtractionService _extractionService;
    private readonly ILogger<InvoiceController> _logger;

    public InvoiceController(
        IInvoiceExtractionService extractionService,
        ILogger<InvoiceController> logger)
    {
        _extractionService = extractionService;
        _logger = logger;
    }

    /// <summary>
    /// Extract structured data from a plain-text invoice or receipt.
    /// </summary>
    /// <remarks>
    /// Set <c>useFineTunedModel: true</c> (default) to include few-shot
    /// training examples in the prompt context. Set to <c>false</c> to
    /// use only the base system prompt for comparison.
    /// </remarks>
    [HttpPost("extract")]
    [ProducesResponseType(typeof(ExtractionResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Extract(
        [FromBody] ExtractionRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.InvoiceText))
            return BadRequest(new { error = "invoiceText must not be empty." });

        _logger.LogInformation(
            "Extraction request | Mode: {Mode} | TextLength: {Len}",
            request.UseFineTunedModel ? "fine-tuned" : "base",
            request.InvoiceText.Length);

        var result = await _extractionService.ExtractAsync(request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns the invoice extraction system prompt (useful for debugging).
    /// </summary>
    [HttpGet("prompt")]
    public IActionResult GetSystemPrompt() =>
        Ok(new { systemPrompt = InvoicePrompts.SystemPrompt });
}
