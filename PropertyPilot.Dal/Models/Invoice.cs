using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table(nameof(Invoice))]
public class Invoice
{
    public static class InvoiceStatuses
    {
        public const string Draft = "Draft";
        public const string Pending = "Pending";
        public const string Paid = "Paid";
        public const string Outstanding = "Outstanding";
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid TenancyId { get; set; } // FK to Tenancy
    public double? Discount { get; set; }
    public Guid TenantId { get; set; } // FK to Tenant
    public DateTime DateStart { get; set; }
    public DateTime? DateDue { get; set; }
    public string InvoiceStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public async Task<double> TotalAmountMinusDiscountAsync(PmsDbContext pmsDbContext)
    {
        var invoiceItems = await pmsDbContext.InvoiceItems.Where(item => item.InvoiceId == this.Id).ToListAsync();
        var totalAmount = invoiceItems.Sum(item => item.Amount);
        if (this.Discount.HasValue) totalAmount -= this.Discount.Value;
        return totalAmount;
    }

    public async Task<double> TotalAmountRemainingAsync(PmsDbContext pmsDbContext)
    {
        var amount = await this.TotalAmountMinusDiscountAsync(pmsDbContext);
        var sumPaid = await pmsDbContext.RentPayments
            .Where(x => x.InvoiceId == this.Id)
            .SumAsync(x => x.Amount);
        var amountRemaining = amount - sumPaid;
        return amountRemaining;
    }
}
