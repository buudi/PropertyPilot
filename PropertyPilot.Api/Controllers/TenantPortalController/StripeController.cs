using Microsoft.AspNetCore.Mvc;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Services.Extensions;
using Stripe;
using Stripe.Checkout;

[ApiController]
[Route("api/stripe")]
public class StripeController : ControllerBase
{
    private readonly PmsDbContext dbContext;
    private readonly IConfiguration configuration;

    public StripeController(PmsDbContext pmsDbContext, IConfiguration configuration)
    {
        this.dbContext = pmsDbContext;
        this.configuration = configuration;
    }

    [HttpPost("tenant-invoice/create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] List<Guid> invoiceIds)
    {
        var invoices = dbContext.Invoices.Where(x => invoiceIds.Contains(x.Id)).ToList();
        var tenantId = invoices.First().TenantId;
        var tenant = dbContext.Tenants.FirstOrDefault(x => x.Id == tenantId);

        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];

        var lineItems = new List<SessionLineItemOptions>();
        foreach (var inv in invoices)
        {
            var amount = await inv.TotalAmountMinusDiscount(dbContext);
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(amount * 100), // Stripe expects fils (1 AED = 100 fils)
                    Currency = "aed",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = $"Invoice {inv.DateStart:dd-MM-yyyy}"
                    }
                },
                Quantity = 1
            });
        }

        var options = new SessionCreateOptions
        {
            SuccessUrl = configuration["Stripe:SuccessUrl"],
            CancelUrl = configuration["Stripe:CancelUrl"],
            LineItems = lineItems,
            Mode = "payment",
            CustomerEmail = tenant?.Email, // Optional: set customer email
            PaymentMethodTypes = new List<string> { "card" },
            Metadata = new Dictionary<string, string>
            {
                { "tenantId", tenantId.ToString() },
                { "invoiceIds", string.Join(",", invoiceIds) }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        // Save a StripePaymentSession record in your DB (not shown here)

        return Ok(new { sessionId = session.Id, url = session.Url });
    }
}
