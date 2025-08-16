﻿using System.ComponentModel.DataAnnotations;

namespace ShiftsLoggerV2.RyanW84.Models;

public class Shift
{
    [Key] public int ShiftId { get; set; }

    public int WorkerId { get; set; }
    public int LocationId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    // Navigation property to the Location entity
    public virtual Location? Location { get; set; }

    // Navigation property to the Worker entity
    public virtual Worker? Worker { get; set; }
}