using System.ComponentModel.DataAnnotations;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Models;

public class Worker : IEntity
{
    [Key] public int WorkerId { get; set; }

    public string Name { get; set; } = string.Empty;

    [Phone] public string? PhoneNumber { get; set; }

    [EmailAddress] public string? Email { get; set; }

    // IEntity implementation
    public int Id => WorkerId;
    // ...existing code...
}