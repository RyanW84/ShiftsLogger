using System.ComponentModel.DataAnnotations;

namespace ShiftsLoggerV2.RyanW84.Models;

public class Location
{
    [Key]
    public int LocationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string PostCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
