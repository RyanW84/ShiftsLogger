using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public class ShiftUI : IShiftUi
{
    private readonly IConsoleDisplayService _display;

    public ShiftUI(IConsoleDisplayService display)
    {
        _display = display;
    }

    public Shift CreateShiftUi(int workerId)
    {
        _display.DisplayHeader("Create New Shift");
        
    var start = AnsiConsole.Ask<DateTimeOffset>("[green]Enter shift start (yyyy-MM-dd HH:mm zzz):[/]");
    var end = AnsiConsole.Ask<DateTimeOffset>("[green]Enter shift end (yyyy-MM-dd HH:mm zzz):[/]");
        var locationId = AnsiConsole.Ask<int>("[green]Enter location ID:[/]");
        
        return new Shift
        {
            ShiftId = 0, // Will be assigned by service
            WorkerId = workerId,
            LocationId = locationId,
            StartTime = start,
            EndTime = end
        };
    }

    public Shift UpdateShiftUi(Shift existingShift)
    {
        _display.DisplayHeader($"Update Shift ID: {existingShift.Id}");
        
    var start = AnsiConsole.Ask<DateTimeOffset>("[green]Enter shift start (yyyy-MM-dd HH:mm zzz):[/]", existingShift.Start);
    var end = AnsiConsole.Ask<DateTimeOffset>("[green]Enter shift end (yyyy-MM-dd HH:mm zzz):[/]", existingShift.End);
        var locationId = AnsiConsole.Ask<int>("[green]Enter location ID:[/]", existingShift.LocationId);
        
        return new Shift
        {
            ShiftId = existingShift.Id,
            WorkerId = existingShift.WorkerId,
            LocationId = locationId,
            StartTime = start,
            EndTime = end
        };
    }

    public ShiftFilterOptions FilterShiftsUi()
    {
        _display.DisplayHeader("Filter Shifts");
        
        var workerId = AnsiConsole.Ask<int?>("[yellow]Filter by worker ID (press Enter to skip):[/]", null);
        var locationId = AnsiConsole.Ask<int?>("[yellow]Filter by location ID (press Enter to skip):[/]", null);
        var startDate = AnsiConsole.Ask<DateTime?>("[yellow]Filter by start date (yyyy-MM-dd, press Enter to skip):[/]", null);
        var endDate = AnsiConsole.Ask<DateTime?>("[yellow]Filter by end date (yyyy-MM-dd, press Enter to skip):[/]", null);
        
        return new ShiftFilterOptions
        {
            WorkerId = workerId,
            LocationId = locationId,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public void DisplayShiftsTable(IEnumerable<Shift> shifts)
    {
        _display.DisplayTable(shifts, "Shifts");
    }

    public int GetShiftByIdUi()
    {
        return AnsiConsole.Ask<int>("[green]Enter shift ID:[/]");
    }
}