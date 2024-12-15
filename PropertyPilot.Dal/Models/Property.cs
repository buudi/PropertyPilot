using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table("properties")]
public class Property
{
    public static class PropertyTypes
    {
        public const string Whole = nameof(Whole);
        public const string Singles = nameof(Singles);
    }

    [Column("id")]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Column("property_name")]
    [Required]
    public required string PropertyName { get; set; }

    [Column("emirate")]
    public string? Emirate { get; set; }

    [Column("property_type")]
    [Required]
    public required string PropertyType { get; set; }

    [Column("units_count")]
    public int? UnitsCount { get; set; }

    [Column("created_on")]
    [Required]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [Column("is_archived")]
    [Required]
    [DefaultValue(false)]
    public bool IsArchived { get; set; } = false;

    [Column("date_archived")]
    public DateTime? DateArchived { get; set; } = null;

    //public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
