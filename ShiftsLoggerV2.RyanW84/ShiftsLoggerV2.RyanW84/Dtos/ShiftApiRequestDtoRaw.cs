using System.ComponentModel.DataAnnotations;

namespace ShiftsLoggerV2.RyanW84.Dtos;

public class ShiftApiRequestDtoRaw
{
    [Required] [Range(1, 255)] public int WorkerId { get; set; }

    [Required]
    public string StartTime { get; set; } = string.Empty;

    [Required]
    public string EndTime { get; set; } = string.Empty;

    [Required] [Range(1, 255)] public int LocationId { get; set; }
}
