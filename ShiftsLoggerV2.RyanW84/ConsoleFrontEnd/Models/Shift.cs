using System.ComponentModel.DataAnnotations;

namespace ConsoleFrontEnd.Models;

public class Shift
{
    [Key] public int ShiftId { get; set; }

    public int WorkerId { get; set; }
    public int LocationId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    // For compatibility with UI layer
    public int Id => ShiftId;
    public DateTime Start 
    { 
        get => StartTime.DateTime;
        set => StartTime = new DateTimeOffset(value);
    }
    public DateTime End 
    { 
        get => EndTime.DateTime;
        set => EndTime = new DateTimeOffset(value);
    }

    // Navigation property to the Location entity
    public virtual Location? Location { get; set; }

    // Navigation property to the Worker entity
    public virtual Worker? Worker { get; set; }
}