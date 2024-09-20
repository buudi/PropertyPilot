using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

[Table(nameof(PropertyListing))]
public class PropertyListing
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public string PropertyName { get; set; } = string.Empty;

    public string? Emirate { get; set; }

    [Required]
    public string PropertyType { get; set; } = string.Empty;

    public int? UnitsCount { get; set; }

    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedOn { get; set; }

    [Required]
    [DefaultValue(false)]
    public bool IsArchived { get; set; }

    public DateTime? DateArchived { get; set; } = null;


}
