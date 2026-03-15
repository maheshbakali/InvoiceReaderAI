using InvoiceAI.Models;

namespace InvoiceAI.Data;

/// <summary>
/// Generates realistic sample invoices used as training data for the LLM.
/// In a production scenario these would be replaced by real labelled invoices.
/// </summary>
public static class SampleInvoiceDataset
{
    public static List<TrainingExample> GetAllExamples() =>
        new()
        {
            GetTechServicesInvoice(),
            GetRetailReceiptExample(),
            GetFreelanceInvoiceExample(),
            GetSaaSSubscriptionInvoice(),
            GetConstructionInvoice(),
            GetRestaurantReceipt(),
            GetMedicalInvoice(),
            GetShippingInvoice(),
        };

    // ──────────────────────────────────────────────────────────────
    // Example 1 – IT / Consulting firm
    // ──────────────────────────────────────────────────────────────
    public static TrainingExample GetTechServicesInvoice() => new()
    {
        Id = "train-001",
        Category = "technology-services",
        RawText = """
            ACME TECH SOLUTIONS LLC
            123 Innovation Drive, Suite 400
            San Francisco, CA 94105
            Tel: (415) 555-0100  |  billing@acmetech.com

            INVOICE

            Invoice Number: INV-2024-0892
            Invoice Date:   March 15, 2024
            Due Date:       April 14, 2024
            Payment Terms:  Net 30

            BILL TO:
            Globex Corporation
            742 Evergreen Terrace
            Springfield, IL 62701

            ─────────────────────────────────────────────────────────
            DESCRIPTION                     QTY   UNIT PRICE   TOTAL
            ─────────────────────────────────────────────────────────
            Cloud Infrastructure Setup        1    $2,500.00   $2,500.00
            Backend API Development          40      $150.00   $6,000.00
            DevOps Consulting               10      $200.00   $2,000.00
            Security Audit & Report           1    $1,200.00   $1,200.00
            ─────────────────────────────────────────────────────────
                                                  Subtotal:  $11,700.00
                                                  Tax (8%):     $936.00
                                              TOTAL DUE:  $12,636.00
            ─────────────────────────────────────────────────────────

            Please remit payment via bank transfer to:
            Account: 1234567890  |  Routing: 021000021

            Thank you for your business!
            """,
        GroundTruth = new Invoice
        {
            VendorName = "ACME TECH SOLUTIONS LLC",
            VendorAddress = "123 Innovation Drive, Suite 400, San Francisco, CA 94105",
            InvoiceNumber = "INV-2024-0892",
            InvoiceDate = new DateTime(2024, 3, 15),
            DueDate = new DateTime(2024, 4, 14),
            PaymentTerms = "Net 30",
            CustomerName = "Globex Corporation",
            CustomerAddress = "742 Evergreen Terrace, Springfield, IL 62701",
            SubTotal = 11700m,
            TaxAmount = 936m,
            TotalAmount = 12636m,
            Currency = "USD",
            Type = InvoiceType.Invoice,
            LineItems = new List<LineItem>
            {
                new() { Description = "Cloud Infrastructure Setup",  Quantity = 1,  UnitPrice = 2500m,  TotalPrice = 2500m },
                new() { Description = "Backend API Development",     Quantity = 40, UnitPrice = 150m,   TotalPrice = 6000m },
                new() { Description = "DevOps Consulting",           Quantity = 10, UnitPrice = 200m,   TotalPrice = 2000m },
                new() { Description = "Security Audit & Report",     Quantity = 1,  UnitPrice = 1200m,  TotalPrice = 1200m },
            }
        },
        PromptCompletion = BuildPromptCompletion("INV-2024-0892")
    };

    // ──────────────────────────────────────────────────────────────
    // Example 2 – Retail receipt
    // ──────────────────────────────────────────────────────────────
    public static TrainingExample GetRetailReceiptExample() => new()
    {
        Id = "train-002",
        Category = "retail-receipt",
        RawText = """
            *** BEST BUY STORE #1047 ***
            1234 Commerce Blvd
            Austin, TX 78701

            Date: 01/22/2024   Time: 14:35
            Transaction: TXN-98234781
            Cashier: EMILY K

            ─────────────────────────────
            Samsung 55" QLED TV    $799.99
            HDMI Cable 6ft           $12.99
            TV Wall Mount            $49.99
            4-Year Protection Plan   $89.99
            ─────────────────────────────
            Subtotal               $952.96
            Tax (8.25%)             $78.62
            ─────────────────────────────
            TOTAL                $1,031.58
            ─────────────────────────────
            Visa ending 4242     $1,031.58

            THANK YOU FOR SHOPPING WITH US!
            Returns accepted within 15 days.
            """,
        GroundTruth = new Invoice
        {
            VendorName = "BEST BUY STORE #1047",
            VendorAddress = "1234 Commerce Blvd, Austin, TX 78701",
            InvoiceNumber = "TXN-98234781",
            InvoiceDate = new DateTime(2024, 1, 22),
            SubTotal = 952.96m,
            TaxAmount = 78.62m,
            TotalAmount = 1031.58m,
            Currency = "USD",
            Type = InvoiceType.Receipt,
            LineItems = new List<LineItem>
            {
                new() { Description = "Samsung 55\" QLED TV",     Quantity = 1, UnitPrice = 799.99m, TotalPrice = 799.99m },
                new() { Description = "HDMI Cable 6ft",           Quantity = 1, UnitPrice = 12.99m,  TotalPrice = 12.99m  },
                new() { Description = "TV Wall Mount",            Quantity = 1, UnitPrice = 49.99m,  TotalPrice = 49.99m  },
                new() { Description = "4-Year Protection Plan",   Quantity = 1, UnitPrice = 89.99m,  TotalPrice = 89.99m  },
            }
        },
        PromptCompletion = BuildPromptCompletion("TXN-98234781")
    };

    // ──────────────────────────────────────────────────────────────
    // Example 3 – Freelance / creative services
    // ──────────────────────────────────────────────────────────────
    public static TrainingExample GetFreelanceInvoiceExample() => new()
    {
        Id = "train-003",
        Category = "freelance",
        RawText = """
            SARAH CHEN DESIGN STUDIO
            Portland, OR  |  sarah@chendesign.co

            Invoice #: SC-2024-047
            Date: February 8, 2024
            Due: February 22, 2024

            To: Startup Ventures Inc.
                500 Market Street, San Francisco, CA 94105

            Services Rendered:
            ───────────────────────────────────────────────────────
            Brand Identity Package (logo, colors, typography)
              1 project @ $3,000.00                       $3,000.00

            Website UI/UX Design (8 screens)
              8 screens @ $350.00/screen                  $2,800.00

            Social Media Asset Pack (20 assets)
              20 items @ $50.00/item                      $1,000.00

            Revision Rounds (included: 3 rounds)
              1 package @ $0.00                               $0.00
            ───────────────────────────────────────────────────────
            Subtotal                                      $6,800.00
            Tax (0% – out-of-state client)                    $0.00
            ───────────────────────────────────────────────────────
            AMOUNT DUE                                    $6,800.00

            Payment via PayPal: payments@chendesign.co
            Late fee of 1.5%/month applies after due date.
            """,
        GroundTruth = new Invoice
        {
            VendorName = "SARAH CHEN DESIGN STUDIO",
            InvoiceNumber = "SC-2024-047",
            InvoiceDate = new DateTime(2024, 2, 8),
            DueDate = new DateTime(2024, 2, 22),
            CustomerName = "Startup Ventures Inc.",
            CustomerAddress = "500 Market Street, San Francisco, CA 94105",
            SubTotal = 6800m,
            TaxAmount = 0m,
            TotalAmount = 6800m,
            Currency = "USD",
            Type = InvoiceType.Invoice,
            LineItems = new List<LineItem>
            {
                new() { Description = "Brand Identity Package",       Quantity = 1,  UnitPrice = 3000m, TotalPrice = 3000m },
                new() { Description = "Website UI/UX Design",         Quantity = 8,  UnitPrice = 350m,  TotalPrice = 2800m },
                new() { Description = "Social Media Asset Pack",      Quantity = 20, UnitPrice = 50m,   TotalPrice = 1000m },
                new() { Description = "Revision Rounds",              Quantity = 1,  UnitPrice = 0m,    TotalPrice = 0m    },
            }
        },
        PromptCompletion = BuildPromptCompletion("SC-2024-047")
    };

    // ──────────────────────────────────────────────────────────────
    // Example 4 – SaaS Subscription
    // ──────────────────────────────────────────────────────────────
    public static TrainingExample GetSaaSSubscriptionInvoice() => new()
    {
        Id = "train-004",
        Category = "saas-subscription",
        RawText = """
            Stripe  |  receipt@stripe.com

            Receipt for payment
            Amount paid: $299.00 USD
            Date paid: Dec 1, 2023

            INVOICE #: 5C6B-0099
            BILLED TO: Acme Corp (billing@acmecorp.com)

            ─────────────────────────────────────────────
            DESCRIPTION                            AMOUNT
            ─────────────────────────────────────────────
            Pro Plan – December 2023              $249.00
            Additional Seats (5 × $10)             $50.00
            ─────────────────────────────────────────────
            Subtotal                              $299.00
            Tax                                    $0.00
            Total                                $299.00
            Amount charged                       $299.00
            ─────────────────────────────────────────────
            Visa •••• 4242
            """,
        GroundTruth = new Invoice
        {
            VendorName = "Stripe",
            InvoiceNumber = "5C6B-0099",
            InvoiceDate = new DateTime(2023, 12, 1),
            CustomerName = "Acme Corp",
            SubTotal = 299m,
            TaxAmount = 0m,
            TotalAmount = 299m,
            Currency = "USD",
            Type = InvoiceType.Receipt,
            LineItems = new List<LineItem>
            {
                new() { Description = "Pro Plan – December 2023",  Quantity = 1, UnitPrice = 249m, TotalPrice = 249m },
                new() { Description = "Additional Seats",          Quantity = 5, UnitPrice = 10m,  TotalPrice = 50m  },
            }
        },
        PromptCompletion = BuildPromptCompletion("5C6B-0099")
    };

    // ──────────────────────────────────────────────────────────────
    // Example 5 – Construction / trades
    // ──────────────────────────────────────────────────────────────
    public static TrainingExample GetConstructionInvoice() => new()
    {
        Id = "train-005",
        Category = "construction",
        RawText = """
            RELIABLE BUILDERS INC.
            890 Contractor Way, Denver, CO 80202
            License #: CON-123456

            INVOICE

            Invoice: RB-5541
            Date: March 1, 2024
            Due: March 31, 2024   (Net 30)

            Project: Kitchen Renovation
            Client: John & Mary Smith
                    4521 Oak Lane, Denver, CO 80203

            MATERIALS & LABOR
            ─────────────────────────────────────────────────────
            Cabinetry (maple, soft-close)   $4,200.00
            Countertops (quartz, 42 sq ft)
              42 sq ft × $85/sq ft          $3,570.00
            Backsplash tile installation
              35 sq ft × $12/sq ft            $420.00
            Labor – Carpentry (16 hrs)
              16 hrs × $95/hr               $1,520.00
            Labor – Plumbing (4 hrs)
              4 hrs × $120/hr                 $480.00
            Permit fees                        $350.00
            ─────────────────────────────────────────────────────
            Subtotal                        $10,540.00
            Sales Tax (4.5%)                   $474.30
            ─────────────────────────────────────────────────────
            TOTAL DUE                       $11,014.30

            50% deposit received: –$5,507.15
            BALANCE DUE:                    $5,507.15
            ─────────────────────────────────────────────────────
            """,
        GroundTruth = new Invoice
        {
            VendorName = "RELIABLE BUILDERS INC.",
            VendorAddress = "890 Contractor Way, Denver, CO 80202",
            InvoiceNumber = "RB-5541",
            InvoiceDate = new DateTime(2024, 3, 1),
            DueDate = new DateTime(2024, 3, 31),
            PaymentTerms = "Net 30",
            CustomerName = "John & Mary Smith",
            CustomerAddress = "4521 Oak Lane, Denver, CO 80203",
            SubTotal = 10540m,
            TaxAmount = 474.30m,
            TotalAmount = 11014.30m,
            Notes = "50% deposit received: –$5,507.15. Balance due: $5,507.15",
            Currency = "USD",
            Type = InvoiceType.Invoice,
            LineItems = new List<LineItem>
            {
                new() { Description = "Cabinetry (maple, soft-close)",         Quantity = 1,  UnitPrice = 4200m,  TotalPrice = 4200m  },
                new() { Description = "Countertops (quartz, 42 sq ft)",        Quantity = 42, UnitPrice = 85m,    TotalPrice = 3570m  },
                new() { Description = "Backsplash tile installation (35sqft)", Quantity = 35, UnitPrice = 12m,    TotalPrice = 420m   },
                new() { Description = "Labor – Carpentry",                     Quantity = 16, UnitPrice = 95m,    TotalPrice = 1520m  },
                new() { Description = "Labor – Plumbing",                      Quantity = 4,  UnitPrice = 120m,   TotalPrice = 480m   },
                new() { Description = "Permit fees",                           Quantity = 1,  UnitPrice = 350m,   TotalPrice = 350m   },
            }
        },
        PromptCompletion = BuildPromptCompletion("RB-5541")
    };

    // ──────────────────────────────────────────────────────────────
    // Example 6 – Restaurant receipt
    // ──────────────────────────────────────────────────────────────
    public static TrainingExample GetRestaurantReceipt() => new()
    {
        Id = "train-006",
        Category = "restaurant",
        RawText = """
            THE GOLDEN FORK
            287 Main Street, Chicago, IL 60601
            (312) 555-0199

            Table: 12   Server: JAMES
            Date: 11/05/2023   Time: 7:48 PM
            Check #: 2023-11050087

            ─────────────────────────
            Ribeye Steak (12oz)  $48.00
            Grilled Salmon       $34.00
            Caesar Salad (x2)    $22.00
            Truffle Fries        $12.00
            Bottle Malbec        $55.00
            Sparkling Water       $8.00
            ─────────────────────────
            Subtotal            $179.00
            Tax (9%)             $16.11
            ─────────────────────────
            Total               $195.11
            Gratuity (18%)       $35.12
            ─────────────────────────
            GRAND TOTAL         $230.23

            AMEX •••• 1001     $230.23
            THANK YOU!
            """,
        GroundTruth = new Invoice
        {
            VendorName = "THE GOLDEN FORK",
            VendorAddress = "287 Main Street, Chicago, IL 60601",
            InvoiceNumber = "2023-11050087",
            InvoiceDate = new DateTime(2023, 11, 5),
            SubTotal = 179m,
            TaxAmount = 16.11m,
            TotalAmount = 230.23m,
            Notes = "Gratuity 18%: $35.12",
            Currency = "USD",
            Type = InvoiceType.Receipt,
            LineItems = new List<LineItem>
            {
                new() { Description = "Ribeye Steak (12oz)",  Quantity = 1, UnitPrice = 48m,  TotalPrice = 48m  },
                new() { Description = "Grilled Salmon",       Quantity = 1, UnitPrice = 34m,  TotalPrice = 34m  },
                new() { Description = "Caesar Salad",         Quantity = 2, UnitPrice = 11m,  TotalPrice = 22m  },
                new() { Description = "Truffle Fries",        Quantity = 1, UnitPrice = 12m,  TotalPrice = 12m  },
                new() { Description = "Bottle Malbec",        Quantity = 1, UnitPrice = 55m,  TotalPrice = 55m  },
                new() { Description = "Sparkling Water",      Quantity = 1, UnitPrice = 8m,   TotalPrice = 8m   },
            }
        },
        PromptCompletion = BuildPromptCompletion("2023-11050087")
    };

    // ──────────────────────────────────────────────────────────────
    // Example 7 – Medical / healthcare
    // ──────────────────────────────────────────────────────────────
    public static TrainingExample GetMedicalInvoice() => new()
    {
        Id = "train-007",
        Category = "medical",
        RawText = """
            CITY MEDICAL CENTER
            Patient Financial Services
            1000 Health Ave, Boston, MA 02115

            STATEMENT OF ACCOUNT

            Statement #: MED-78432
            Statement Date: January 10, 2024
            Due Date: February 9, 2024

            Patient: Robert Johnson
            DOB: 05/14/1978
            Account #: PAT-00023881

            ─────────────────────────────────────────────────────
            SERVICE DATE   DESCRIPTION                     AMOUNT
            ─────────────────────────────────────────────────────
            12/20/2023     Office Visit (Level 3)          $180.00
            12/20/2023     Blood Panel – Comprehensive      $85.00
            12/20/2023     EKG – 12 Lead                    $95.00
            12/20/2023     Flu Vaccination                  $40.00
            ─────────────────────────────────────────────────────
                           Subtotal                        $400.00
                           Insurance Adjustment           –$120.00
                           Patient Responsibility          $280.00
                           Tax                               $0.00
            ─────────────────────────────────────────────────────
                           BALANCE DUE                     $280.00
            """,
        GroundTruth = new Invoice
        {
            VendorName = "CITY MEDICAL CENTER",
            VendorAddress = "1000 Health Ave, Boston, MA 02115",
            InvoiceNumber = "MED-78432",
            InvoiceDate = new DateTime(2024, 1, 10),
            DueDate = new DateTime(2024, 2, 9),
            CustomerName = "Robert Johnson",
            SubTotal = 400m,
            TaxAmount = 0m,
            TotalAmount = 280m,
            Notes = "Insurance Adjustment: –$120.00",
            Currency = "USD",
            Type = InvoiceType.Invoice,
            LineItems = new List<LineItem>
            {
                new() { Description = "Office Visit (Level 3)",          Quantity = 1, UnitPrice = 180m, TotalPrice = 180m },
                new() { Description = "Blood Panel – Comprehensive",     Quantity = 1, UnitPrice = 85m,  TotalPrice = 85m  },
                new() { Description = "EKG – 12 Lead",                   Quantity = 1, UnitPrice = 95m,  TotalPrice = 95m  },
                new() { Description = "Flu Vaccination",                 Quantity = 1, UnitPrice = 40m,  TotalPrice = 40m  },
            }
        },
        PromptCompletion = BuildPromptCompletion("MED-78432")
    };

    // ──────────────────────────────────────────────────────────────
    // Example 8 – Shipping / logistics
    // ──────────────────────────────────────────────────────────────
    public static TrainingExample GetShippingInvoice() => new()
    {
        Id = "train-008",
        Category = "shipping-logistics",
        RawText = """
            FedEx FREIGHT  |  fedex.com
            Invoice Date: 2024-02-14
            Invoice Number: FX-INV-20240214-0045

            BILLED TO:
            Omega Retail LLC
            9000 Warehouse Way
            Dallas, TX 75201

            ─────────────────────────────────────────────────────────────
            TRACKING #      DESCRIPTION                           AMOUNT
            ─────────────────────────────────────────────────────────────
            7489134820492   Ground Delivery (5.2 lbs, 3 pkgs)    $28.75
            7489134820493   Priority Overnight (1.1 lbs)         $42.50
            7489134820495   Freight LTL 200 lbs                 $310.00
                            Residential Surcharge (×2)            $14.50
                            Fuel Surcharge (8.5%)                $32.78
            ─────────────────────────────────────────────────────────────
            Subtotal                                            $428.53
            Tax                                                  $0.00
            TOTAL DUE                                           $428.53
            ─────────────────────────────────────────────────────────────
            Terms: Net 15
            """,
        GroundTruth = new Invoice
        {
            VendorName = "FedEx FREIGHT",
            InvoiceNumber = "FX-INV-20240214-0045",
            InvoiceDate = new DateTime(2024, 2, 14),
            PaymentTerms = "Net 15",
            CustomerName = "Omega Retail LLC",
            CustomerAddress = "9000 Warehouse Way, Dallas, TX 75201",
            SubTotal = 428.53m,
            TaxAmount = 0m,
            TotalAmount = 428.53m,
            Currency = "USD",
            Type = InvoiceType.Invoice,
            LineItems = new List<LineItem>
            {
                new() { Description = "Ground Delivery (5.2 lbs, 3 pkgs)",  Quantity = 1, UnitPrice = 28.75m,  TotalPrice = 28.75m  },
                new() { Description = "Priority Overnight (1.1 lbs)",       Quantity = 1, UnitPrice = 42.50m,  TotalPrice = 42.50m  },
                new() { Description = "Freight LTL 200 lbs",                Quantity = 1, UnitPrice = 310m,    TotalPrice = 310m    },
                new() { Description = "Residential Surcharge",              Quantity = 2, UnitPrice = 7.25m,   TotalPrice = 14.50m  },
                new() { Description = "Fuel Surcharge (8.5%)",              Quantity = 1, UnitPrice = 32.78m,  TotalPrice = 32.78m  },
            }
        },
        PromptCompletion = BuildPromptCompletion("FX-INV-20240214-0045")
    };

    // ──────────────────────────────────────────────────────────────
    // Shared helper – builds the prompt/completion pair
    // ──────────────────────────────────────────────────────────────
    private static PromptCompletion BuildPromptCompletion(string invoiceNumber) => new()
    {
        SystemPrompt = InvoicePrompts.SystemPrompt,
        UserMessage = $"Please extract all invoice fields from the following invoice text and return valid JSON only.\n\nInvoice text:\n[INVOICE_TEXT_PLACEHOLDER]",
        AssistantResponse = $"{{\"invoiceNumber\":\"{invoiceNumber}\",\"extraction\":\"see ground truth JSON\"}}"
    };
}
