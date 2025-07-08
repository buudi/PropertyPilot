using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
using PropertyPilot.Services.Constants;
using PropertyPilot.Services.Extensions;
using PropertyPilot.Services.FinanceServices;
using PropertyPilot.Services.FinanceServices.Models;
using Stripe;
using Stripe.Checkout;
using System.Text;

[ApiController]
[Route("api/stripe")]
public class StripeController(PmsDbContext pmsDbContext, IConfiguration configuration, FinancesService financesService, ILogger<StripeController> logger) : ControllerBase
{
    private readonly Guid _stripeUserAccountGuid = Keys.StripeUserAccountGuid;

    [HttpPost("tenant-invoice/create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] List<Guid> invoiceIds)
    {
        var invoices = pmsDbContext.Invoices.Where(x => invoiceIds.Contains(x.Id)).ToList();
        var tenantId = invoices.First().TenantId;
        var tenant = pmsDbContext.Tenants.FirstOrDefault(x => x.Id == tenantId);

        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];

        var lineItems = new List<SessionLineItemOptions>();
        foreach (var inv in invoices)
        {
            var amount = await inv.TotalAmountMinusDiscount(pmsDbContext);
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(amount * 100), // (1 AED = 100 fils)
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
            CustomerEmail = tenant?.Email,
            PaymentMethodTypes = new List<string> { "card" },
            Metadata = new Dictionary<string, string>
            {
                { "tenantId", tenantId.ToString() },
                { "invoiceIds", string.Join(",", invoiceIds) }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        var paymentSession = new StripePaymentSession
        {
            Id = Guid.NewGuid(),
            StripeSessionId = session.Id,
            TenantId = tenantId,
            InvoiceIds = string.Join(",", invoiceIds),
            TotalAmount = (double)lineItems.Sum(x => (x.PriceData.UnitAmount / 100.00) ?? 0),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        pmsDbContext.StripePaymentSessions.Add(paymentSession);
        await pmsDbContext.SaveChangesAsync();

        return Ok(new { sessionId = session.Id, url = session.Url });
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var stripeSignature = Request.Headers["Stripe-Signature"];
        var endpointSecret = configuration["Stripe:WebhookSecret"];

        HttpContext.Request.EnableBuffering();

        string json;
        using (var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            json = await reader.ReadToEndAsync();
            HttpContext.Request.Body.Position = 0; //reset position for potential re-reading
        }

        Event stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, endpointSecret, throwOnApiVersionMismatch: false);

        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

        if (stripeEvent.Type == Events.CheckoutSessionCompleted)
        {
            var paymentSession = pmsDbContext.StripePaymentSessions.FirstOrDefault(x => x.StripeSessionId == session.Id);

            if (paymentSession.Status == "Completed")
            {
                return Ok(new { message = "Payment session already completed: {SessionId}", sessionId = session.Id });
            }


            var invoiceIds = paymentSession.InvoiceIds.Split(',').Select(id =>
            {
                if (Guid.TryParse(id.Trim(), out var guid))
                    return guid;
                throw new FormatException($"Invalid GUID format: {id}");
            }).ToList();

            foreach (var invoiceId in invoiceIds)
            {
                var invoice = pmsDbContext.Invoices.FirstOrDefault(x => x.Id == invoiceId);
                if (invoice == null)
                {
                    return NotFound($"Invoice not found: {invoiceId}");
                }
                var amount = await invoice.TotalAmountMinusDiscount(pmsDbContext);
                var rentPaymentRequest = new RentPaymentRequest
                {
                    TenantId = invoice.TenantId,
                    InvoiceId = invoice.Id,
                    Amount = amount,
                    PaymentMethod = RentPayment.PaymentMethods.StripePayment
                };
                await financesService.RecordRentPayment(_stripeUserAccountGuid, rentPaymentRequest);
            }

            paymentSession.Status = "Completed";
            paymentSession.StripePaymentIntentId = session.PaymentIntentId;
            paymentSession.CompletedAt = DateTime.UtcNow;

            await pmsDbContext.SaveChangesAsync();

            return Ok(new { message = "Payment Completed!", paymentSession, invoiceIds });
        }


        return Ok(new { message = "Webhook received successfully!", stripeEvent });
    }

    [HttpPost("test-record-stripe-payments")]
    public async Task<IActionResult> TestRecordStripePayments([FromBody] List<Guid> invoiceIds)
    {
        foreach (var invoiceId in invoiceIds)
        {
            var invoice = pmsDbContext.Invoices.FirstOrDefault(x => x.Id == invoiceId);
            if (invoice == null)
            {
                return NotFound($"Invoice not found: {invoiceId}");
            }
            var amount = await invoice.TotalAmountMinusDiscount(pmsDbContext);
            var rentPaymentRequest = new RentPaymentRequest
            {
                TenantId = invoice.TenantId,
                InvoiceId = invoice.Id,
                Amount = amount,
                PaymentMethod = RentPayment.PaymentMethods.StripePayment
            };
            await financesService.RecordRentPayment(_stripeUserAccountGuid, rentPaymentRequest);
        }
        return Ok(new { message = "Test rent payments recorded for all invoices." });
    }

    [HttpGet("payment-session/{sessionId}")]
    public async Task<IActionResult> GetPaymentSession(string sessionId)
    {
        var paymentSession = await pmsDbContext.StripePaymentSessions
            .FirstOrDefaultAsync(x => x.StripeSessionId == sessionId);
        if (paymentSession == null)
            return NotFound("Payment session not found.");
        return Ok(paymentSession);
    }
}
