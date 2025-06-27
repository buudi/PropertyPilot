using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyPilot.Dal.Contexts;
using PropertyPilot.Dal.Models;
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
    public async Task<IActionResult> StripeWebhook()
    {
        // Disable buffering to ensure we get the raw body
        HttpContext.Request.EnableBuffering();

        // Read the raw request body as bytes, not string
        string json;
        using (var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            json = await reader.ReadToEndAsync();
            HttpContext.Request.Body.Position = 0; // Reset position for potential re-reading
        }

        var stripeSignature = Request.Headers["Stripe-Signature"];
        var endpointSecret = configuration["Stripe:WebhookSecret"];

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, endpointSecret, throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            logger.LogError($"Stripe webhook signature verification failed: {ex.Message}");
            return BadRequest($"Webhook signature verification failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Webhook processing error: {ex.Message}");
            return BadRequest($"Webhook error: {ex.Message}");
        }

        if (stripeEvent.Type == Events.CheckoutSessionCompleted)
        {
            try
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                if (session == null)
                {
                    return BadRequest("Could not deserialize checkout session");
                }

                logger.LogInformation("Processing session: {SessionId}", session.Id);

                var paymentSession = pmsDbContext.StripePaymentSessions.FirstOrDefault(x => x.StripeSessionId == session.Id);
                if (paymentSession == null)
                {
                    logger.LogWarning("Payment session not found for Stripe session: {SessionId}", session.Id);
                    return Ok(); // Not an error - might be a test webhook or duplicate
                }

                if (paymentSession.Status == "Completed")
                {
                    logger.LogInformation("Payment session already completed: {SessionId}", session.Id);
                    return Ok();
                }

                logger.LogInformation("Processing invoices: {InvoiceIds}", paymentSession.InvoiceIds);

                // Check if InvoiceIds is null or empty
                if (string.IsNullOrEmpty(paymentSession.InvoiceIds))
                {
                    logger.LogWarning("No invoice IDs found in payment session");
                    return BadRequest("No invoice IDs found in payment session");
                }

                var invoiceIds = paymentSession.InvoiceIds.Split(',').Select(id =>
                {
                    if (Guid.TryParse(id.Trim(), out var guid))
                        return guid;
                    throw new FormatException($"Invalid GUID format: {id}");
                }).ToList();

                foreach (var invoiceId in invoiceIds)
                {
                    logger.LogInformation("Processing invoice: {InvoiceId}", invoiceId);

                    var invoice = pmsDbContext.Invoices.FirstOrDefault(x => x.Id == invoiceId);
                    if (invoice == null)
                    {
                        logger.LogWarning("Invoice not found: {InvoiceId}", invoiceId);
                        continue;
                    }

                    var amount = await invoice.TotalAmountMinusDiscount(pmsDbContext);
                    logger.LogInformation("Invoice amount: {Amount}", amount);

                    var rentPaymentRequest = new RentPaymentRequest
                    {
                        TenantId = invoice.TenantId,
                        InvoiceId = invoice.Id,
                        Amount = amount,
                        PaymentMethod = RentPayment.PaymentMethods.StripePayment
                    };

                    await financesService.RecordRentPayment(Guid.Empty, rentPaymentRequest);
                    logger.LogInformation("Payment recorded for invoice: {InvoiceId}", invoiceId);
                }

                paymentSession.Status = "Completed";
                paymentSession.StripePaymentIntentId = session.PaymentIntentId;
                paymentSession.CompletedAt = DateTime.UtcNow;

                try
                {
                    await pmsDbContext.SaveChangesAsync();
                    logger.LogInformation("Payment session marked as completed: {SessionId}", session.Id);
                }
                catch (Exception saveEx)
                {
                    logger.LogError(saveEx, "Error saving payment session changes: {Message}", saveEx.Message);

                    // Log inner exceptions
                    var innerEx = saveEx.InnerException;
                    while (innerEx != null)
                    {
                        logger.LogError("Save inner exception: {Message}", innerEx.Message);
                        innerEx = innerEx.InnerException;
                    }
                    throw; // Re-throw to be caught by outer catch block
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing webhook: {Message}", ex.Message);

                // Log inner exceptions for Entity Framework errors
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    logger.LogError("Inner exception: {Message}", innerEx.Message);
                    innerEx = innerEx.InnerException;
                }

                logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                return StatusCode(500, $"Error processing webhook: {ex.Message}");
            }
        }

        return Ok();
    }

    [HttpPost("webhook-test")]
    [AllowAnonymous] // Ensure no auth is required
    public IActionResult WebhookTest()
    {
        // Log or print something simple if you want
        var stripeSignature = Request.Headers["Stripe-Signature"];
        return Ok(new { message = "Webhook received successfully!", stripeSignature });
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
