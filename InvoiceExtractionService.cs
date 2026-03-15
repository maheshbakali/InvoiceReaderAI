using InvoiceAI.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace InvoiceAI.Services;

/// <summary>Contract for the invoice extraction service.</summary>
public interface IInvoiceExtractionService
{
    Task<ExtractionResponse> ExtractAsync(ExtractionRequest request, CancellationToken ct = default);
    void SetFewShotExamples(List<(string Raw, string Json)> examples);
}

/// <summary>
/// Calls the Anthropic Claude API to extract structured data from invoice text.
///
/// When <see cref="ExtractionRequest.UseFineTunedModel"/> is true the service
/// prepends few-shot training examples to the system prompt, simulating the
/// behaviour of a fine-tuned model.
/// </summary>
public class AnthropicInvoiceExtractionService : IInvoiceExtractionService
{
    private readonly ILogger<AnthropicInvoiceExtractionService> _logger;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    private List<(string Raw, string Json)> _fewShotExamples = new();

    private const string AnthropicApiUrl = "https://api.anthropic.com/v1/messages";
    private const string ModelId = "claude-opus-4-5";

    public AnthropicInvoiceExtractionService(
        ILogger<AnthropicInvoiceExtractionService> logger,
        IConfiguration config,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _config = config;
        _httpClient = httpClientFactory.CreateClient("anthropic");
    }

    public void SetFewShotExamples(List<(string Raw, string Json)> examples)
    {
        _fewShotExamples = examples;
        _logger.LogInformation("Loaded {Count} few-shot training examples into extraction service", examples.Count);
    }

    public async Task<ExtractionResponse> ExtractAsync(ExtractionRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            string systemPrompt = request.UseFineTunedModel && _fewShotExamples.Count > 0
                ? InvoicePrompts.BuildFewShotSystemPrompt(_fewShotExamples)
                : InvoicePrompts.SystemPrompt;

            string userMessage = InvoicePrompts.BuildExtractionPrompt(request.InvoiceText);

            _logger.LogInformation(
                "Calling Anthropic API | Mode: {Mode} | FewShot examples: {Count}",
                request.UseFineTunedModel ? "fine-tuned" : "base",
                _fewShotExamples.Count);

            string rawResponse = await CallAnthropicAsync(systemPrompt, userMessage, ct);

            Invoice? invoice = ParseInvoiceFromJson(rawResponse, request.SourceFileName);

            sw.Stop();

            return new ExtractionResponse
            {
                Success            = invoice is not null,
                Invoice            = invoice,
                RawModelResponse   = rawResponse,
                ProcessingTimeMs   = sw.ElapsedMilliseconds,
                ErrorMessage       = invoice is null ? "Could not parse JSON from model response" : null
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Invoice extraction failed");
            return new ExtractionResponse
            {
                Success          = false,
                ErrorMessage     = ex.Message,
                ProcessingTimeMs = sw.ElapsedMilliseconds
            };
        }
    }

    // ── Anthropic API call ────────────────────────────────────────────────────

    private async Task<string> CallAnthropicAsync(
        string systemPrompt, string userMessage, CancellationToken ct)
    {
        string apiKey = _config["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException(
                "Anthropic API key not configured. Set 'Anthropic:ApiKey' in appsettings or environment variable 'ANTHROPIC_API_KEY'.");

        var payload = new
        {
            model      = ModelId,
            max_tokens = 4096,
            system     = systemPrompt,
            messages   = new[] { new { role = "user", content = userMessage } }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, AnthropicApiUrl)
        {
            Content = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json")
        };

        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        HttpResponseMessage response = await _httpClient.SendAsync(request, ct);
        string body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Anthropic API error {(int)response.StatusCode}: {body}");

        dynamic parsed = JsonConvert.DeserializeObject<dynamic>(body)!;
        return (string)parsed.content[0].text;
    }

    // ── JSON parsing ──────────────────────────────────────────────────────────

    private Invoice? ParseInvoiceFromJson(string rawResponse, string? sourceFile)
    {
        try
        {
            // Strip markdown code fences if present
            string json = ExtractJsonBlock(rawResponse);

            dynamic? obj = JsonConvert.DeserializeObject<dynamic>(json);
            if (obj is null) return null;

            var invoice = new Invoice
            {
                VendorName      = (string?)obj.vendorName,
                VendorAddress   = (string?)obj.vendorAddress,
                InvoiceNumber   = (string?)obj.invoiceNumber,
                InvoiceDate     = ParseDate((string?)obj.invoiceDate),
                DueDate         = ParseDate((string?)obj.dueDate),
                PaymentTerms    = (string?)obj.paymentTerms,
                CustomerName    = (string?)obj.customerName,
                CustomerAddress = (string?)obj.customerAddress,
                Currency        = (string?)obj.currency ?? "USD",
                SubTotal        = ParseDecimal(obj.subTotal),
                TaxAmount       = ParseDecimal(obj.taxAmount),
                TotalAmount     = ParseDecimal(obj.totalAmount),
                Notes           = (string?)obj.notes,
                Type            = ParseInvoiceType((string?)obj.invoiceType),
                Metadata        = new ExtractionMetadata
                {
                    ConfidenceScore = (double?)obj.confidenceScore ?? 0.8,
                    SourceFile      = sourceFile,
                    WasFineTuned    = _fewShotExamples.Count > 0,
                    ModelVersion    = ModelId
                }
            };

            // Parse line items
            if (obj.lineItems is not null)
            {
                foreach (var item in obj.lineItems)
                {
                    invoice.LineItems.Add(new LineItem
                    {
                        Description = (string?)item.description,
                        Quantity    = ParseDecimal(item.quantity),
                        UnitPrice   = ParseDecimal(item.unitPrice),
                        TotalPrice  = ParseDecimal(item.totalPrice),
                        SKU         = (string?)item.sku,
                        Category    = (string?)item.category
                    });
                }
            }

            return invoice;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse invoice JSON from response");
            return null;
        }
    }

    private static string ExtractJsonBlock(string text)
    {
        // Remove ```json ... ``` or ``` ... ``` fences
        var match = Regex.Match(text, @"```(?:json)?\s*([\s\S]*?)```", RegexOptions.IgnoreCase);
        if (match.Success) return match.Groups[1].Value.Trim();

        // Find the first { ... } block
        int start = text.IndexOf('{');
        int end   = text.LastIndexOf('}');
        if (start >= 0 && end > start) return text[start..(end + 1)];

        return text.Trim();
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return DateTime.TryParse(value, out var dt) ? dt : null;
    }

    private static decimal ParseDecimal(dynamic? value)
    {
        if (value is null) return 0m;
        return decimal.TryParse(value.ToString(), out decimal d) ? d : 0m;
    }

    private static InvoiceType ParseInvoiceType(string? value) => value?.ToLower() switch
    {
        "receipt"       => InvoiceType.Receipt,
        "creditnote"    => InvoiceType.CreditNote,
        "purchaseorder" => InvoiceType.PurchaseOrder,
        _               => InvoiceType.Invoice
    };
}
