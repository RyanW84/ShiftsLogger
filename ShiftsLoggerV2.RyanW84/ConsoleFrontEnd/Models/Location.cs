using System.ComponentModel.DataAnnotations;

namespace ConsoleFrontEnd.Models;

public class Location
{
    [Key] public int LocationId { get; set; }

    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string Town { get; set; }
    public required string County { get; set; }
    public required string PostCode { get; set; }
    public required string Country { get; set; }

    public virtual Shift? Shifts { get; set; } // Navigation property to the Shifts entity
    public virtual Worker? Workers { get; set; } // Navigation property to the Workers entity
}