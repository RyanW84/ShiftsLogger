using System.ComponentModel.DataAnnotations;

namespace ShiftsLoggerV2.RyanW84.Models;

public class Worker
{
    [Key] public int WorkerId { get; set; }

    public string Name { get; set; } = string.Empty;

    [Phone] public string? PhoneNumber { get; set; }

    [EmailAddress] public string? Email { get; set; }
}