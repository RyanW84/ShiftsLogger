using System.ComponentModel.DataAnnotations;

namespace ConsoleFrontEnd.Models;

public class Worker
{
    [Key] public int WorkerId { get; set; }

    public string Name { get; set; } = string.Empty;

    [Phone] public string? PhoneNumber { get; set; }

    [EmailAddress] public string? Email { get; set; }

    // For compatibility with UI layer
    public int Id => WorkerId;
    public string? Phone => PhoneNumber;
    
    // For backward compatibility - split Name into first/last for display
    public string FirstName 
    { 
        get 
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : string.Empty;
        }
        set
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
                Name = $"{value} {string.Join(' ', parts.Skip(1))}";
            else
                Name = value;
        }
    }
    
    public string LastName 
    { 
        get 
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : string.Empty;
        }
        set
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
                Name = $"{parts[0]} {value}";
            else
                Name = value;
        }
    }

    public virtual ICollection<Shift>? Shifts { get; set; }
    public virtual ICollection<Location>? Locations { get; set; }
}