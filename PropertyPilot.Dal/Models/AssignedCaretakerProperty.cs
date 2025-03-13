using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PropertyPilot.Dal.Models;

public class AssignedCaretakerProperty
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // FK to PropertyPilotUser
    public Guid PropertyListingId { get; set; } // FK to PropertyListing
}
