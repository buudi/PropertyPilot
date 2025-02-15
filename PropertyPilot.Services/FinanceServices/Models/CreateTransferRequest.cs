using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.FinanceServices.Models;

public record CreateTransferRequest
{
    [Required]
    public Guid SourceAccountId { get; set; }

    [Required]
    public Guid DestinationAccountId { get; set; }

    [Required]
    public double Amount { get; set; } = 0.0;
}