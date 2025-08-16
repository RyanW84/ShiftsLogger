using System.ComponentModel.DataAnnotations;

namespace ConsoleFrontEnd.Models;

public class Worker
{
    [Key] public int WorkerId { get; set; }

    public string Name { get; set; }

    [Phone] public string? PhoneNumber { get; set; }

    [EmailAddress] public string? Email { get; set; }

    public virtual Shift? Shifts { get; set; }
    public virtual Location? Locations { get; set; }
}