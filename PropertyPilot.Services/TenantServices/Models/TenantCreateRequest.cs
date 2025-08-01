﻿using PropertyPilot.Services.Attributes.Validation;
using System.ComponentModel.DataAnnotations;

namespace PropertyPilot.Services.TenantServices.Models;

public record TenantCreateRequest
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string PhoneNumber { get; set; }

    [Required]
    public required string Email { get; set; }

    [Required]
    public required string TenantIdentification { get; set; }

    [Required]
    public required bool IsAccountActive { get; set; }

    [Required]
    public required DateTime TenancyStart { get; set; }

    [Required]
    public required bool IsInvoiceRenewable { get; set; }
    public int? RenewalPeriodInDays { get; set; }

    public DateTime? TenancyEnd { get; set; }

    [Required]
    public required Guid PropertyUnitId { get; set; }

    [GuidOrEmpty]
    public Guid SubUnitId { get; set; } = Guid.Empty;

    [Required]
    public required double AssignedRent { get; set; }

    public double? OneTimeDiscount { get; set; }
}
