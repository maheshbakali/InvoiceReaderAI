using InvoiceAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceAI.Controllers;

/// <summary>
/// Training management endpoints – trigger training runs, inspect datasets,
/// and export training files.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TrainingController : ControllerBase
{
    private readonly ModelTrainingService _trainingService;
    private readonly ILogger<TrainingController> _logger;

    public TrainingController(
        ModelTrainingService trainingService,
        ILogger<TrainingController> logger)
    {
        _trainingService = trainingService;
        _logger = logger;
    }

    /// <summary>
    /// Returns statistics about the loaded training dataset.
    /// </summary>
    [HttpGet("stats")]
    public IActionResult GetStats() =>
        Ok(_trainingService.GetStats());

    /// <summary>
    /// Runs the full training pipeline:
    /// 1. Loads sample invoices
    /// 2. Builds few-shot prompt examples
    /// 3. Evaluates schema conformance
    /// 4. Registers trained context with the extraction service
    /// </summary>
    [HttpPost("run")]
    public async Task<IActionResult> RunTraining(CancellationToken ct)
    {
        _logger.LogInformation("Training run initiated via API");
        var result = await _trainingService.TrainAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Exports the training dataset as JSONL (OpenAI fine-tune format).
    /// Useful if you want to submit real fine-tuning jobs to a model provider.
    /// </summary>
    [HttpGet("export/jsonl")]
    [Produces("text/plain")]
    public IActionResult ExportJsonl()
    {
        string jsonl = _trainingService.ExportTrainingJsonl();
        return Content(jsonl, "text/plain");
    }
}
