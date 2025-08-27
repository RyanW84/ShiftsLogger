using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Threading.Tasks;

namespace ConsoleFrontEnd.MenuSystem;

public class ShiftUI : IShiftUi
{
    private readonly IConsoleDisplayService _display;
    private readonly UiHelper _uiHelper;
    private readonly ShiftInputHelper _shiftInputHelper;
    private readonly ConsoleFrontEnd.Interfaces.IShiftService _shiftService;

    public ShiftUI(IConsoleDisplayService display, ILogger<ShiftUI> logger, ShiftInputHelper shiftInputHelper, ConsoleFrontEnd.Interfaces.IShiftService shiftService)
    {
        _display = display;
        _uiHelper = new UiHelper(display, logger);
        _shiftInputHelper = shiftInputHelper ?? throw new ArgumentNullException(nameof(shiftInputHelper));
        _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
    }

    public async Task<Shift> CreateShiftUi(int workerId)
    {
        _display.DisplayHeader("Create New Shift");

        var start = _shiftInputHelper.GetDateTimeInput("Start Time");
        var end = _shiftInputHelper.GetDateTimeInput("End Time");
        var locationId = await _shiftInputHelper.SelectLocationAsync(null, false).ConfigureAwait(false);

        return new Shift
        {
            ShiftId = 0, // Will be assigned by service
            WorkerId = workerId,
            LocationId = locationId,
            StartTime = start,
            EndTime = end
        };
    }

    public async Task<Shift> UpdateShiftUi(Shift existingShift)
    {
        _display.DisplayHeader($"Update Shift ID: {existingShift.Id}");

        var start = _shiftInputHelper.GetDateTimeInput("Start Time", existingShift.Start, true);
        var end = _shiftInputHelper.GetDateTimeInput("End Time", existingShift.End, true);
        var locationId = await _shiftInputHelper.SelectLocationAsync(existingShift.LocationId, true).ConfigureAwait(false);

        return new Shift
        {
            ShiftId = existingShift.Id,
            WorkerId = existingShift.WorkerId,
            LocationId = locationId,
            StartTime = start,
            EndTime = end
        };
    }

    public async Task<ShiftFilterOptions> FilterShiftsUi()
    {
        _display.DisplayHeader("Filter Shifts");

        int? workerId = null;
        var filterByWorker = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Filter by worker?")
                .AddChoices(new[] { "No", "Yes" })
        );
        if (filterByWorker == "Yes")
            workerId = await _shiftInputHelper.SelectWorkerAsync(null, false).ConfigureAwait(false);

        int? locationId = null;
        var filterByLocation = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Filter by location?")
                .AddChoices(new[] { "No", "Yes" })
        );
        if (filterByLocation == "Yes")
            locationId = await _shiftInputHelper.SelectLocationAsync(null, false).ConfigureAwait(false);

        DateTime? startDate = null;
        DateTime? endDate = null;
        var wantDates = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Filter by date range?")
                .AddChoices(new[] { "No", "Yes" })
        );
        if (wantDates == "Yes")
        {
            startDate = _shiftInputHelper.GetDateTimeInput("Start Date").DateTime;
            endDate = _shiftInputHelper.GetDateTimeInput("End Date").DateTime;
        }

        int? minDurationMinutes = null;
        int? maxDurationMinutes = null;
        var wantDuration = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Filter by duration?")
                .AddChoices(new[] { "No", "Yes" })
        );
        if (wantDuration == "Yes")
        {
            var minDurationInput = AnsiConsole.Ask<string>("Minimum duration in minutes (press Enter to skip):", "");
            if (!string.IsNullOrWhiteSpace(minDurationInput) && int.TryParse(minDurationInput, out var minDuration) && minDuration > 0)
            {
                minDurationMinutes = minDuration;
            }

            var maxDurationInput = AnsiConsole.Ask<string>("Maximum duration in minutes (press Enter to skip):", "");
            if (!string.IsNullOrWhiteSpace(maxDurationInput) && int.TryParse(maxDurationInput, out var maxDuration) && maxDuration > 0)
            {
                maxDurationMinutes = maxDuration;
            }
        }

        return new ShiftFilterOptions
        {
            WorkerId = workerId,
            LocationId = locationId,
            StartDate = startDate,
            EndDate = endDate,
            MinDurationMinutes = minDurationMinutes,
            MaxDurationMinutes = maxDurationMinutes
        };
    }

    public void DisplayShiftsTable(IEnumerable<Shift> shifts)
    {
        _display.DisplayTable(shifts, "Shifts");
    }

    public async Task<int> GetShiftByIdUi()
    {
    _display.DisplayHeader("Select Shift", "blue");

        var response = await _shiftService.GetAllShiftsAsync().ConfigureAwait(false);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            _uiHelper.DisplayValidationError(response.Message ?? "No shifts available.");
            // Fallback to manual entry
            return AnsiConsole.Ask<int>("[green]Enter shift ID:[/]");
        }

        var choices = response.Data
            .Select(s => $"{s.ShiftId}: {s.StartTime:dd/MM/yyyy HH:mm} - {s.EndTime:dd/MM/yyyy HH:mm} ({s.Duration.TotalHours:F1}h)")
            .ToArray();

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select Shift:")
                .AddChoices(choices)
        );

        return UiHelper.ExtractIdFromChoice(selected);
    }
}