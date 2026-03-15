namespace InvoiceAI.Models;

/// <summary>
/// Represents a labelled training example used to fine-tune the invoice extraction model.
/// </summary>
public class TrainingExample
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Raw invoice text (OCR output or plain-text invoice).</summary>
    public string RawText { get; set; } = string.Empty;

    /// <summary>Ground-truth JSON extracted invoice fields.</summary>
    public Invoice GroundTruth { get; set; } = new();

    /// <summary>The few-shot prompt pair used during fine-tuning.</summary>
    public PromptCompletion PromptCompletion { get; set; } = new();

    public string Category { get; set; } = "general";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A prompt/completion pair in the OpenAI fine-tune JSONL format
/// (also used as few-shot examples for Claude).
/// </summary>
public class PromptCompletion
{
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public string AssistantResponse { get; set; } = string.Empty;
}

/// <summary>Result summary from a training run.</summary>
public class TrainingResult
{
    public string TrainingId { get; set; } = Guid.NewGuid().ToString();
    public int TotalExamples { get; set; }
    public int SuccessfulExamples { get; set; }
    public double AverageConfidence { get; set; }
    public List<TrainingMetric> Metrics { get; set; } = new();
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public string ModelVersion { get; set; } = string.Empty;
    public bool IsSuccessful => SuccessfulExamples > 0;
}

public class TrainingMetric
{
    public string FieldName { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public int SampleCount { get; set; }
}

/// <summary>
/// Request DTO for the extraction endpoint.
/// </summary>
public class ExtractionRequest
{
    /// <summary>Plain-text content of the invoice (paste or OCR result).</summary>
    public string InvoiceText { get; set; } = string.Empty;

    /// <summary>Whether to use the fine-tuned few-shot examples.</summary>
    public bool UseFineTunedModel { get; set; } = true;

    /// <summary>Optional filename/source label for metadata.</summary>
    public string? SourceFileName { get; set; }
}

/// <summary>
/// Response DTO from the extraction endpoint.
/// </summary>
public class ExtractionResponse
{
    public bool Success { get; set; }
    public Invoice? Invoice { get; set; }
    public string? ErrorMessage { get; set; }
    public string RawModelResponse { get; set; } = string.Empty;
    public long ProcessingTimeMs { get; set; }
}
