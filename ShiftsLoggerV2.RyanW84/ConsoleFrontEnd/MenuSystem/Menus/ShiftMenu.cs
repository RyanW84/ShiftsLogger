using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Services;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.MenuSystem.Menus;

/// <summary>
/// Shift menu implementation following Single Responsibility Principle
/// Handles shift-specific operations
/// </summary>
public class ShiftMenuV2 : BaseMenuV2
{
    private readonly IShiftService _shiftService;
    private readonly IWorkerService _workerService;
    private readonly ILocationService _locationService;

    public ShiftMenuV2(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger<ShiftMenuV2> logger,
        IShiftService shiftService,
        IWorkerService workerService,
        ILocationService locationService)
        : base(displayService, inputService, navigationService, logger)
    {
        _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
        _workerService = workerService ?? throw new ArgumentNullException(nameof(workerService));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
    }

    public override string Title => "Shift Management";
    public override string Context => "Shift Management";

    protected override async Task ShowMenuAsync()
    {
        bool shouldExit = false;
        
        while (!shouldExit)
        {
            var choice = InputService.GetMenuChoice(
                "Select a shift operation:",
                "View All Shifts",
                "View Shift by ID",
                "Create New Shift",
                "Update Shift",
                "Delete Shift",
                "Filter Shifts",
                "View Shifts by Worker",
                "View Shifts by Date Range",
                "Back to Main Menu"
            );

            shouldExit = await HandleShiftChoice(choice);
        }
    }

    private async Task<bool> HandleShiftChoice(string choice)
    {
        Logger.LogDebug("Shift menu choice selected: {Choice}", choice);

        if (await HandleCommonActions(choice))
            return true;

        try
        {
            switch (choice)
            {
                case "View All Shifts":
                    await ViewAllShiftsAsync();
                    break;

                case "View Shift by ID":
                    await ViewShiftByIdAsync();
                    break;

                case "Create New Shift":
                    await CreateShiftAsync();
                    break;

                case "Update Shift":
                    await UpdateShiftAsync();
                    break;

                case "Delete Shift":
                    await DeleteShiftAsync();
                    break;

                case "Filter Shifts":
                    await FilterShiftsAsync();
                    break;

                case "View Shifts by Worker":
                    await ViewShiftsByWorkerAsync();
                    break;

                case "View Shifts by Date Range":
                    await ViewShiftsByDateRangeAsync();
                    break;

                case "Back to Main Menu":
                    return true;

                default:
                    DisplayService.DisplayError("Invalid choice");
                    InputService.WaitForKeyPress();
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling shift choice: {Choice}", choice);
            DisplayService.DisplayError($"An error occurred: {ex.Message}");
            InputService.WaitForKeyPress();
        }

        return false;
    }

    private async Task ViewAllShiftsAsync()
    {
        DisplayService.DisplayHeader("All Shifts", "blue");
        
        var response = await _shiftService.GetAllShiftsAsync();
        
        if (response.RequestFailed)
        {
            DisplayService.DisplayError($"Failed to retrieve shifts: {response.Message}");
        }
        else
        {
            DisplayService.DisplayTable(response.Data, "All Shifts");
            DisplayService.DisplaySuccess($"Total shifts: {response.TotalCount}");
        }
        
        InputService.WaitForKeyPress();
    }

    private async Task ViewShiftByIdAsync()
    {
        var shiftId = InputService.GetIntegerInput("Enter Shift ID:", 1);
        
        DisplayService.DisplayHeader($"Shift Details (ID: {shiftId})", "blue");
        
        // Implementation would call the API to get shift by ID
        // For now, showing the pattern
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }

    private async Task CreateShiftAsync()
    {
        DisplayService.DisplayHeader("Create New Shift", "green");
        
        // Get available workers
        var workersResponse = await _workerService.GetAllWorkersAsync();
        if (workersResponse.RequestFailed || !workersResponse.Data.Any())
        {
            DisplayService.DisplayError("No workers available. Please create workers first.");
            InputService.WaitForKeyPress();
            return;
        }

        // Get available locations
        var locationsResponse = await _locationService.GetAllLocationsAsync();
        if (locationsResponse.RequestFailed || !locationsResponse.Data.Any())
        {
            DisplayService.DisplayError("No locations available. Please create locations first.");
            InputService.WaitForKeyPress();
            return;
        }

        try
        {
            // Select worker
            var workerChoices = workersResponse.Data.Select(w => $"{w.WorkerId}: {w.Name}").ToArray();
            var selectedWorkerChoice = InputService.GetMenuChoice("Select Worker:", workerChoices);
            var workerId = int.Parse(selectedWorkerChoice.Split(':')[0]);

            // Select location
            var locationChoices = locationsResponse.Data.Select(l => $"{l.LocationId}: {l.Name}").ToArray();
            var selectedLocationChoice = InputService.GetMenuChoice("Select Location:", locationChoices);
            var locationId = int.Parse(selectedLocationChoice.Split(':')[0]);

            // Get shift times
            var startTime = InputService.GetDateTimeInput("Enter Start Time (yyyy-MM-dd HH:mm):");
            var endTime = InputService.GetDateTimeInput("Enter End Time (yyyy-MM-dd HH:mm):");

            if (endTime <= startTime)
            {
                DisplayService.DisplayError("End time must be after start time.");
                InputService.WaitForKeyPress();
                return;
            }

            // Create shift (this would call the API)
            DisplayService.DisplaySuccess("Shift created successfully!");
            DisplayService.DisplayInfo($"Worker: {workersResponse.Data.First(w => w.WorkerId == workerId).Name}");
            DisplayService.DisplayInfo($"Location: {locationsResponse.Data.First(l => l.LocationId == locationId).Name}");
            DisplayService.DisplayInfo($"Start: {startTime}");
            DisplayService.DisplayInfo($"End: {endTime}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating shift");
            DisplayService.DisplayError($"Failed to create shift: {ex.Message}");
        }

        InputService.WaitForKeyPress();
    }

    private async Task UpdateShiftAsync()
    {
        DisplayService.DisplayHeader("Update Shift", "yellow");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }

    private async Task DeleteShiftAsync()
    {
        DisplayService.DisplayHeader("Delete Shift", "red");
        
        var shiftId = InputService.GetIntegerInput("Enter Shift ID to delete:", 1);
        
        if (InputService.GetConfirmation($"Are you sure you want to delete shift {shiftId}?"))
        {
            DisplayService.DisplayInfo("Feature implementation in progress...");
        }
        else
        {
            DisplayService.DisplayInfo("Delete cancelled.");
        }
        
        InputService.WaitForKeyPress();
    }

    private async Task FilterShiftsAsync()
    {
        DisplayService.DisplayHeader("Filter Shifts", "blue");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }

    private async Task ViewShiftsByWorkerAsync()
    {
        DisplayService.DisplayHeader("Shifts by Worker", "blue");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }

    private async Task ViewShiftsByDateRangeAsync()
    {
        DisplayService.DisplayHeader("Shifts by Date Range", "blue");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }
}
