using InvoiceAI.Data;
using InvoiceAI.Models;
using InvoiceAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceAI.Controllers;

/// <summary>
/// Demo endpoints that run extractions against the built-in sample invoices
/// without requiring the caller to supply invoice text.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DemoController : ControllerBase
{
    private readonly IInvoiceExtractionService _extractionService;

    public DemoController(IInvoiceExtractionService extractionService) =>
        _extractionService = extractionService;

    /// <summary>Lists all available sample invoice IDs and categories.</summary>
    [HttpGet("samples")]
    public IActionResult ListSamples() =>
        Ok(SampleInvoiceDataset.GetAllExamples()
            .Select(e => new { e.Id, e.Category })
            .ToList());

    /// <summary>
    /// Runs extraction on a specific sample invoice by ID.
    /// IDs: train-001 … train-008
    /// </summary>
    [HttpPost("run/{sampleId}")]
    public async Task<IActionResult> RunSample(
        string sampleId,
        [FromQuery] bool useFineTunedModel = true,
        CancellationToken ct = default)
    {
        var example = SampleInvoiceDataset.GetAllExamples()
            .FirstOrDefault(e => e.Id.Equals(sampleId, StringComparison.OrdinalIgnoreCase));

        if (example is null)
            return NotFound(new { error = $"Sample '{sampleId}' not found. Valid IDs: train-001 to train-008." });

        var request = new ExtractionRequest
        {
            InvoiceText       = example.RawText,
            UseFineTunedModel = useFineTunedModel,
            SourceFileName    = $"{sampleId}.txt"
        };

        var response = await _extractionService.ExtractAsync(request, ct);

        // Attach ground-truth for easy comparison
        return Ok(new
        {
            sampleId,
            category          = example.Category,
            extractionResult  = response,
            groundTruth       = example.GroundTruth
        });
    }

    /// <summary>
    /// Compares base-model vs fine-tuned extraction on a sample invoice.
    /// Returns both responses side-by-side.
    /// </summary>
    [HttpPost("compare/{sampleId}")]
    public async Task<IActionResult> CompareModels(
        string sampleId,
        CancellationToken ct = default)
    {
        var example = SampleInvoiceDataset.GetAllExamples()
            .FirstOrDefault(e => e.Id.Equals(sampleId, StringComparison.OrdinalIgnoreCase));

        if (example is null)
            return NotFound(new { error = $"Sample '{sampleId}' not found." });

        var baseRequest = new ExtractionRequest
        {
            InvoiceText       = example.RawText,
            UseFineTunedModel = false,
            SourceFileName    = $"{sampleId}-base.txt"
        };

        var fineTunedRequest = new ExtractionRequest
        {
            InvoiceText       = example.RawText,
            UseFineTunedModel = true,
            SourceFileName    = $"{sampleId}-finetuned.txt"
        };

        var baseTask      = _extractionService.ExtractAsync(baseRequest, ct);
        var fineTunedTask = _extractionService.ExtractAsync(fineTunedRequest, ct);

        await Task.WhenAll(baseTask, fineTunedTask);

        return Ok(new
        {
            sampleId,
            category         = example.Category,
            baseModel        = baseTask.Result,
            fineTunedModel   = fineTunedTask.Result,
            groundTruth      = example.GroundTruth,
            improvement      = new
            {
                baseConfidence      = baseTask.Result.Invoice?.Metadata.ConfidenceScore,
                fineTunedConfidence = fineTunedTask.Result.Invoice?.Metadata.ConfidenceScore,
            }
        });
    }
}
