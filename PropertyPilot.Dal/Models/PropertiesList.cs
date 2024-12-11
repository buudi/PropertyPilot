using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table("properties_list")]
public class PropertiesList
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
    public string PropertyName { get; set; } = string.Empty;

    [Column("emirate")]
    public string? Emirate { get; set; }

    [Column("property_type")]
    [Required]
    public string PropertyType { get; set; } = string.Empty;

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
}
