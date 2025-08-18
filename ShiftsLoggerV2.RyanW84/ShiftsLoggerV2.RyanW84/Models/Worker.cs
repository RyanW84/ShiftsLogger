using System.ComponentModel.DataAnnotations;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Models;

public class Worker : IEntity
{
    [Key] public int WorkerId { get; set; }

    public string Name { get; set; } = string.Empty;

    [Phone] public string? PhoneNumber { get; set; }

    [EmailAddress] public string? Email { get; set; }

    // Navigation property for related shifts
    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

	// For compatibility with UI layer
	public int Id => WorkerId;
	public string? Phone => PhoneNumber;
}