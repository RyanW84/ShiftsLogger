using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public class ShiftUI : IShiftUi
{
    private readonly IConsoleDisplayService _display;
    private readonly UiHelper _uiHelper;

    public ShiftUI(IConsoleDisplayService display, ILogger<ShiftUI> logger)
    {
        _display = display;
        _uiHelper = new UiHelper(display, logger);
    }

    public Shift CreateShiftUi(int workerId)
    {
        _display.DisplayHeader("Create New Shift");

        var start = AnsiConsole.Ask<DateTimeOffset>("[green]Enter shift start (dd/MM/yyyy HH:mm):[/]");
        var end = AnsiConsole.Ask<DateTimeOffset>("[green]Enter shift end (dd/MM/yyyy HH:mm):[/]");
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

        var start = AnsiConsole.Ask<DateTimeOffset>("[green]Enter shift start (dd/MM/yyyy HH:mm):[/]", existingShift.Start);
        var end = AnsiConsole.Ask<DateTimeOffset>("[green]Enter shift end (dd/MM/yyyy HH:mm):[/]", existingShift.End);
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

        var workerId = _uiHelper.GetOptionalIntInput("Filter by worker ID");
        var locationId = _uiHelper.GetOptionalIntInput("Filter by location ID");
        var startDate = _uiHelper.GetOptionalDateTimeInput("Filter by start date");
        var endDate = _uiHelper.GetOptionalDateTimeInput("Filter by end date");

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