﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table(nameof(Expense))]
public class Expense
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid? PropertyListingId { get; set; }
    public Guid PaidByAccountId { get; set; }
    public Guid PaidByUserId { get; set; }
    public string Category { get; set; } = "No Category";
    public string? Description { get; set; }
    public double Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}