﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table(nameof(PropertyPilotUser))]
public class PropertyPilotUser
{
    public static class UserRoles
    {
        public static string AdminManager = nameof(AdminManager);
        public static string Manager = nameof(Manager);
        public static string Caretaker = nameof(Caretaker);
        public static string Tenant = nameof(Tenant);
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [Required]
    public string HashedPassword { get; set; } = string.Empty;

    [Required]
    [DefaultValue(true)]
    public bool HasAccess { get; set; } = true;

    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;

    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedOn { get; set; }

    [Required]
    [DefaultValue(false)]
    public bool IsArchived { get; set; }

    public DateTime? DateArchived { get; set; } = null;

    public string? RefreshToken { get; set; } = string.Empty;

    public DateTime? RefreshTokenExpiryTime { get; set; } = null;
}
