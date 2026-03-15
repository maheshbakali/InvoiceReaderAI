using InvoiceAI.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ─────────────────────────────────────────────────────────────
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Map ANTHROPIC_API_KEY env var → Anthropic:ApiKey config key
var anthropicApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
if (!string.IsNullOrEmpty(anthropicApiKey))
    builder.Configuration["Anthropic:ApiKey"] = anthropicApiKey;

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddNewtonsoftJson(opts =>
    {
        opts.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        opts.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title   = "Invoice AI API",
        Version = "v1",
        Description = """
            Demonstrates training a base LLM to read invoices and receipts
            using few-shot learning and prompt engineering.

            ## Quickstart
            1. **POST /api/training/run** – run the training pipeline
            2. **POST /api/demo/run/train-001** – extract a sample invoice
            3. **POST /api/invoice/extract** – extract your own invoice text
            """
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// HTTP client for Anthropic API
builder.Services.AddHttpClient("anthropic", client =>
{
    client.Timeout = TimeSpan.FromSeconds(120);
    client.BaseAddress = new Uri("https://api.anthropic.com");
});

// Core services
builder.Services.AddSingleton<IInvoiceExtractionService, AnthropicInvoiceExtractionService>();
builder.Services.AddSingleton<ModelTrainingService>();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// ── Pipeline ──────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Invoice AI v1");
        c.RoutePrefix = string.Empty; // serve Swagger UI at root
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ── Auto-train on startup (loads few-shot context into memory) ────────────────
var logger = app.Services.GetRequiredService<ILogger<Program>>();
try
{
    logger.LogInformation("Auto-training model on startup with sample invoice dataset...");
    var trainingService = app.Services.GetRequiredService<ModelTrainingService>();
    await trainingService.TrainAsync();
    logger.LogInformation("Startup training complete. Invoice AI is ready.");
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Startup training failed (API calls will still work with base model)");
}

app.Run();
