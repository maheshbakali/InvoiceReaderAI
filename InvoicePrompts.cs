namespace InvoiceAI;

/// <summary>
/// Centralised prompt engineering for the invoice extraction model.
/// These prompts act as the "training signal" for in-context learning /
/// fine-tuning. Well-crafted prompts are critical for high accuracy.
/// </summary>
public static class InvoicePrompts
{
    /// <summary>
    /// System prompt that sets the model's role and output format.
    /// This is the foundation of our "fine-tuned" behaviour.
    /// </summary>
    public const string SystemPrompt = """
        You are an expert invoice and receipt data extraction specialist with deep knowledge
        of accounting, billing formats, and financial documents from all industries.

        Your task is to extract structured data from invoice and receipt text with extreme precision.

        ## Output Requirements
        - Respond ONLY with valid JSON — no markdown, no explanation, no prose.
        - Use null for any field that cannot be determined from the text.
        - All monetary amounts must be numeric decimals (e.g., 1234.56), never strings.
        - Dates must be in ISO 8601 format: YYYY-MM-DD.
        - invoiceType must be one of: "Invoice", "Receipt", "CreditNote", "PurchaseOrder".

        ## JSON Schema to follow EXACTLY:
        {
          "vendorName": string | null,
          "vendorAddress": string | null,
          "invoiceNumber": string | null,
          "invoiceDate": "YYYY-MM-DD" | null,
          "dueDate": "YYYY-MM-DD" | null,
          "paymentTerms": string | null,
          "customerName": string | null,
          "customerAddress": string | null,
          "currency": string,
          "invoiceType": "Invoice" | "Receipt" | "CreditNote" | "PurchaseOrder",
          "lineItems": [
            {
              "description": string,
              "quantity": number,
              "unitPrice": number,
              "totalPrice": number,
              "sku": string | null,
              "category": string | null
            }
          ],
          "subTotal": number,
          "taxAmount": number,
          "totalAmount": number,
          "notes": string | null,
          "confidenceScore": number  // 0.0 to 1.0, your confidence in the extraction
        }

        ## Rules
        - Extract ALL line items, even if totals don't perfectly sum.
        - For receipts without invoice numbers, use the transaction/check/receipt number.
        - If a vendor address spans multiple lines, concatenate with ", ".
        - Infer currency from symbols ($ = USD, £ = GBP, € = EUR) or explicit text.
        - confidenceScore should reflect how complete and unambiguous the source text is.
        """;

    /// <summary>
    /// Builds the full user message with the invoice text injected.
    /// </summary>
    public static string BuildExtractionPrompt(string invoiceText) =>
        $"""
        Extract all invoice/receipt fields from the following text and return valid JSON only.

        --- INVOICE TEXT START ---
        {invoiceText}
        --- INVOICE TEXT END ---
        """;

    /// <summary>
    /// Few-shot examples prepended when fine-tuned mode is active.
    /// Each example teaches the model a different invoice style/format.
    /// </summary>
    public static string BuildFewShotSystemPrompt(List<(string Raw, string ExpectedJson)> examples)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(SystemPrompt);
        sb.AppendLine();
        sb.AppendLine("## Training Examples (learn from these patterns)");
        sb.AppendLine();

        int i = 1;
        foreach (var (raw, expected) in examples)
        {
            sb.AppendLine($"### Example {i++}");
            sb.AppendLine("**Input:**");
            sb.AppendLine(raw.Length > 800 ? raw[..800] + "..." : raw);
            sb.AppendLine();
            sb.AppendLine("**Expected Output:**");
            sb.AppendLine(expected);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
