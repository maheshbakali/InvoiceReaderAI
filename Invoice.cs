namespace InvoiceAI.Models;

/// <summary>
/// Represents a parsed invoice/receipt with all extracted fields.
/// </summary>
public class Invoice
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? VendorName { get; set; }
    public string? VendorAddress { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public List<LineItem> LineItems { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? PaymentTerms { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? Notes { get; set; }
    public InvoiceType Type { get; set; } = InvoiceType.Invoice;
    public ExtractionMetadata Metadata { get; set; } = new();
}

public class LineItem
{
    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? SKU { get; set; }
    public string? Category { get; set; }
}

public class ExtractionMetadata
{
    public double ConfidenceScore { get; set; }
    public string ModelVersion { get; set; } = "1.0";
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
    public string? SourceFile { get; set; }
    public bool WasFineTuned { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public enum InvoiceType
{
    Invoice,
    Receipt,
    CreditNote,
    PurchaseOrder
}
