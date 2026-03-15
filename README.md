# Invoice AI – LLM-Powered Invoice Extraction (.NET 8)

A production-ready C# .NET 8 project that demonstrates training a base LLM to accurately read, understand, and extract structured data from invoices and receipts of any format.

---

## Architecture Overview

```
InvoiceAI/
├── Controllers/
│   ├── InvoiceController.cs      – POST /api/invoice/extract
│   ├── TrainingController.cs     – POST /api/training/run
│   └── DemoController.cs         – POST /api/demo/run/{id}
├── Services/
│   ├── IInvoiceExtractionService.cs
│   ├── InvoiceExtractionService.cs   – Anthropic Claude API integration
│   └── ModelTrainingService.cs       – Training pipeline orchestration
├── Models/
│   ├── Invoice.cs                – Domain models
│   └── TrainingModels.cs         – Training/request/response DTOs
├── Data/
│   └── SampleInvoiceDataset.cs   – 8 labelled training invoices
├── InvoicePrompts.cs             – System & few-shot prompt engineering
└── Program.cs                    – DI setup + startup auto-training
```

---

## How "Training" Works

This project uses **two complementary LLM training techniques**:

### 1. Few-Shot In-Context Learning
Labelled `(invoice text → extracted JSON)` pairs from the sample dataset are injected into the system prompt at inference time. The LLM learns:
- Your exact JSON output schema
- How to handle different invoice formats (retail, medical, SaaS, construction, etc.)
- Edge cases like deposits, discounts, multi-currency

### 2. Prompt Engineering / Optimisation
The system prompt is carefully crafted to act as the "weights" of the model. The training service evaluates field accuracy across the dataset and selects the highest-performing prompt variant.

### Production Fine-Tuning Path
The `POST /api/training/export/jsonl` endpoint exports a JSONL file in OpenAI fine-tune format. To submit actual weight-update fine-tuning jobs:

```bash
# Export training data
curl https://localhost:5001/api/training/export/jsonl > training.jsonl

# Submit to OpenAI (requires OpenAI API key)
openai api fine_tuning.jobs.create \
  -t training.jsonl \
  -m gpt-4o-mini-2024-07-18
```

---

## Sample Training Dataset

| ID         | Category              | Vendor                  |
|------------|----------------------|-------------------------|
| train-001  | Technology Services   | ACME Tech Solutions LLC |
| train-002  | Retail Receipt        | Best Buy                |
| train-003  | Freelance / Creative  | Sarah Chen Design Studio|
| train-004  | SaaS Subscription     | Stripe                  |
| train-005  | Construction / Trades | Reliable Builders Inc.  |
| train-006  | Restaurant Receipt    | The Golden Fork         |
| train-007  | Medical / Healthcare  | City Medical Center     |
| train-008  | Shipping / Logistics  | FedEx Freight           |

---

## Quick Start

### Prerequisites
- .NET 8 SDK
- An Anthropic API key

### 1. Configure your API key

**Option A – appsettings.json:**
```json
{
  "Anthropic": { "ApiKey": "sk-ant-..." }
}
```

**Option B – Environment variable (recommended):**
```bash
export ANTHROPIC_API_KEY=sk-ant-...
```

### 2. Run the project
```bash
cd InvoiceAI
dotnet run
```
The app starts on `https://localhost:5001`. Swagger UI loads at the root `/`.

### 3. Try the API

**Run training pipeline:**
```bash
curl -X POST https://localhost:5001/api/training/run
```

**Extract a sample invoice (fine-tuned):**
```bash
curl -X POST https://localhost:5001/api/demo/run/train-001
```

**Compare base vs fine-tuned:**
```bash
curl -X POST https://localhost:5001/api/demo/compare/train-002
```

**Extract your own invoice:**
```bash
curl -X POST https://localhost:5001/api/invoice/extract \
  -H "Content-Type: application/json" \
  -d '{
    "invoiceText": "ACME Corp\nInvoice #123\nDate: 2024-01-15\n\nWeb Development  1  $5000\n\nTotal: $5000",
    "useFineTunedModel": true
  }'
```

---

## API Reference

### `POST /api/invoice/extract`
Extract structured data from any invoice text.

**Request:**
```json
{
  "invoiceText": "...",
  "useFineTunedModel": true,
  "sourceFileName": "invoice.txt"
}
```

**Response:**
```json
{
  "success": true,
  "invoice": {
    "vendorName": "ACME Corp",
    "invoiceNumber": "INV-123",
    "invoiceDate": "2024-01-15T00:00:00",
    "totalAmount": 5000.00,
    "currency": "USD",
    "lineItems": [...],
    "metadata": {
      "confidenceScore": 0.97,
      "wasFineTuned": true,
      "modelVersion": "claude-opus-4-5"
    }
  },
  "processingTimeMs": 1423
}
```

### `POST /api/training/run`
Runs the full training pipeline and returns metrics.

### `GET /api/training/stats`
Returns dataset statistics (example counts, categories, coverage).

### `GET /api/training/export/jsonl`
Downloads the training dataset in OpenAI JSONL format.

### `POST /api/demo/run/{sampleId}`
Runs extraction on a built-in sample invoice. IDs: `train-001` to `train-008`.

### `POST /api/demo/compare/{sampleId}`
Runs both base and fine-tuned extractions side-by-side for comparison.

---

## Extending the Training Dataset

Add a new `TrainingExample` to `SampleInvoiceDataset.cs`:

```csharp
public static TrainingExample GetMyNewInvoice() => new()
{
    Id = "train-009",
    Category = "utilities",
    RawText = """
        POWER COMPANY
        Account: 123456
        Amount Due: $145.50
        Due: March 1, 2024
        """,
    GroundTruth = new Invoice
    {
        VendorName = "POWER COMPANY",
        TotalAmount = 145.50m,
        InvoiceDate = new DateTime(2024, 3, 1),
        // ...
    }
};
```

Then register it in `GetAllExamples()`:
```csharp
public static List<TrainingExample> GetAllExamples() => new()
{
    // existing examples...
    GetMyNewInvoice()
};
```

---

## Key Technologies

| Technology | Purpose |
|---|---|
| .NET 8 | Runtime & web framework |
| ASP.NET Core | REST API |
| Anthropic Claude API | Base LLM for extraction |
| Newtonsoft.Json | JSON parsing |
| Swashbuckle | Swagger UI |
| Microsoft.Extensions.ML | ML.NET integration hooks |
