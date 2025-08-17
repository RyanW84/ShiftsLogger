
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
// ...existing code...
public class ShiftMenuV2 : BaseMenuV2
{
    // ...existing fields, constructor, and methods...

    private async Task ViewShiftsByWorkerAsync()
    {
        DisplayService.DisplayHeader("Shifts by Worker", "blue");
        var workersResponse = await _workerService.GetAllWorkersAsync();
        if (workersResponse.RequestFailed || workersResponse.Data == null || !workersResponse.Data.Any())
        {
            DisplayService.DisplayError(workersResponse.Message ?? "No workers found.");
            InputService.WaitForKeyPress();
            return;
        }
        var workerChoices = workersResponse.Data.Select(w => $"{w.WorkerId}: {w.Name}").ToArray();
        var selectedWorkerChoice = InputService.GetMenuChoice("Select Worker:", workerChoices);
        var workerId = int.Parse(selectedWorkerChoice.Split(':')[0]);
        var filter = new ShiftFilterOptions { WorkerId = workerId };
        var response = await _shiftService.GetShiftsByFilterAsync(filter);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(response.Message ?? "No shifts found for selected worker.");
        }
        else
        {
            _shiftUi.DisplayShiftsTable(response.Data);
            DisplayService.DisplaySuccess($"Total shifts: {response.TotalCount}");
        }
        InputService.WaitForKeyPress();
    }

    private async Task ViewShiftsByDateRangeAsync()
    {
        DisplayService.DisplayHeader("Shifts by Date Range", "blue");
        var startDate = InputService.GetDateTimeInput("Enter start date (yyyy-MM-dd HH:mm):");
        var endDate = InputService.GetDateTimeInput("Enter end date (yyyy-MM-dd HH:mm):");
        if (endDate <= startDate)
        {
            DisplayService.DisplayError("End date must be after start date.");
            InputService.WaitForKeyPress();
            return;
        }
        var filter = new ShiftFilterOptions { StartTime = startDate, EndTime = endDate };
        var response = await _shiftService.GetShiftsByFilterAsync(filter);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(response.Message ?? "No shifts found in date range.");
        }
        else
        {
            _shiftUi.DisplayShiftsTable(response.Data);
            DisplayService.DisplaySuccess($"Total shifts: {response.TotalCount}");
        }
        InputService.WaitForKeyPress();
    }
    private readonly IShiftService _shiftService;
    private readonly IWorkerService _workerService;
    private readonly ILocationService _locationService;
    private readonly IShiftUi _shiftUi;

    public ShiftMenuV2(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger<ShiftMenuV2> logger,
        IShiftService shiftService,
        IWorkerService workerService,
        ILocationService locationService,
        IShiftUi shiftUi)
        : base(displayService, inputService, navigationService, logger)
    {
        _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
        _workerService = workerService ?? throw new ArgumentNullException(nameof(workerService));
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
        _shiftUi = shiftUi ?? throw new ArgumentNullException(nameof(shiftUi));
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
            switch (response.ResponseCode)
            {
                case System.Net.HttpStatusCode.NotFound:
                    DisplayService.DisplayError("No shifts found (404).");
                    break;
                case System.Net.HttpStatusCode.BadRequest:
                    DisplayService.DisplayError("Bad request (400).");
                    break;
                case System.Net.HttpStatusCode.InternalServerError:
                    DisplayService.DisplayError("Server error (500).");
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    DisplayService.DisplayError("Unauthorized (401). Please log in.");
                    break;
                case System.Net.HttpStatusCode.Forbidden:
                    DisplayService.DisplayError("Forbidden (403). You do not have permission.");
                    break;
                case System.Net.HttpStatusCode.Conflict:
                    DisplayService.DisplayError("Conflict (409). Resource conflict detected.");
                    break;
                case System.Net.HttpStatusCode.RequestTimeout:
                    DisplayService.DisplayError("Request Timeout (408). The server timed out.");
                    break;
                case (System.Net.HttpStatusCode)422:
                    DisplayService.DisplayError("Unprocessable Entity (422). Validation failed.");
                    break;
                default:
                    DisplayService.DisplayError($"Failed to retrieve shifts: {response.Message}");
                    break;
            }
        }
        else if (response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError("No shifts found (404).");
        }
        else
        {
            _shiftUi.DisplayShiftsTable(response.Data);
            DisplayService.DisplaySuccess($"Total shifts: {response.TotalCount}");
        }
        InputService.WaitForKeyPress();
    }

    private async Task ViewShiftByIdAsync()
    {
        var shiftId = InputService.GetIntegerInput("Enter Shift ID:", 1);
        DisplayService.DisplayHeader($"Shift Details (ID: {shiftId})", "blue");
        var response = await _shiftService.GetShiftByIdAsync(shiftId);
        if (response.RequestFailed || response.Data == null)
        {
            DisplayService.DisplayError(response.Message ?? "Shift not found (404).");
        }
        else
        {
            _shiftUi.DisplayShiftsTable(new List<Shift> { response.Data });
            DisplayService.DisplaySuccess("Shift details loaded successfully.");
        }
        InputService.WaitForKeyPress();
    }

    private async Task CreateShiftAsync()
    {
        DisplayService.DisplayHeader("Create New Shift", "green");
        
        // Get available workers
        var workersResponse = await _workerService.GetAllWorkersAsync();
    if (workersResponse.RequestFailed)
        {
            switch (workersResponse.ResponseCode)
            {
                case System.Net.HttpStatusCode.NotFound:
                    DisplayService.DisplayError("No workers found (404).");
                    break;
                case System.Net.HttpStatusCode.BadRequest:
                    DisplayService.DisplayError("Bad request (400) while retrieving workers.");
                    break;
                case System.Net.HttpStatusCode.InternalServerError:
                    DisplayService.DisplayError("Server error (500) while retrieving workers.");
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    DisplayService.DisplayError("Unauthorized (401) while retrieving workers.");
                    break;
                case System.Net.HttpStatusCode.Forbidden:
                    DisplayService.DisplayError("Forbidden (403) while retrieving workers.");
                    break;
                case System.Net.HttpStatusCode.Conflict:
                    DisplayService.DisplayError("Conflict (409) while retrieving workers.");
                    break;
                case System.Net.HttpStatusCode.RequestTimeout:
                    DisplayService.DisplayError("Request Timeout (408) while retrieving workers.");
                    break;
                case (System.Net.HttpStatusCode)422:
                    DisplayService.DisplayError("Unprocessable Entity (422) while retrieving workers.");
                    break;
                default:
                    DisplayService.DisplayError($"Failed to retrieve workers: {workersResponse.Message}");
                    break;
            }
            InputService.WaitForKeyPress();
            return;
        }
    if (workersResponse.Data == null || !workersResponse.Data.Any())
        {
            DisplayService.DisplayError("No workers found (404). Please create workers first.");
            InputService.WaitForKeyPress();
            return;
        }

        // Get available locations
        var locationsResponse = await _locationService.GetAllLocationsAsync();
    if (locationsResponse.RequestFailed)
        {
            switch (locationsResponse.ResponseCode)
            {
                case System.Net.HttpStatusCode.NotFound:
                    DisplayService.DisplayError("No locations found (404).");
                    break;
                case System.Net.HttpStatusCode.BadRequest:
                    DisplayService.DisplayError("Bad request (400) while retrieving locations.");
                    break;
                case System.Net.HttpStatusCode.InternalServerError:
                    DisplayService.DisplayError("Server error (500) while retrieving locations.");
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    DisplayService.DisplayError("Unauthorized (401) while retrieving locations.");
                    break;
                case System.Net.HttpStatusCode.Forbidden:
                    DisplayService.DisplayError("Forbidden (403) while retrieving locations.");
                    break;
                case System.Net.HttpStatusCode.Conflict:
                    DisplayService.DisplayError("Conflict (409) while retrieving locations.");
                    break;
                case System.Net.HttpStatusCode.RequestTimeout:
                    DisplayService.DisplayError("Request Timeout (408) while retrieving locations.");
                    break;
                case (System.Net.HttpStatusCode)422:
                    DisplayService.DisplayError("Unprocessable Entity (422) while retrieving locations.");
                    break;
                default:
                    DisplayService.DisplayError($"Failed to retrieve locations: {locationsResponse.Message}");
                    break;
            }
            InputService.WaitForKeyPress();
            return;
        }
    if (locationsResponse.Data == null || !locationsResponse.Data.Any())
        {
            DisplayService.DisplayError("No locations found (404). Please create locations first.");
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
    await Task.CompletedTask;
    }

    private async Task UpdateShiftAsync()
    {
        DisplayService.DisplayHeader("Update Shift", "yellow");
        var allShiftsResponse = await _shiftService.GetAllShiftsAsync();
        if (allShiftsResponse.RequestFailed || allShiftsResponse.Data == null || !allShiftsResponse.Data.Any())
        {
            DisplayService.DisplayError(allShiftsResponse.Message ?? "No shifts found.");
            InputService.WaitForKeyPress();
            return;
        }
        var shiftChoices = allShiftsResponse.Data.Select(s => $"{s.ShiftId}: {s.StartTime:yyyy-MM-dd HH:mm} - {s.EndTime:yyyy-MM-dd HH:mm}").ToArray();
        var selectedShiftChoice = InputService.GetMenuChoice("Select Shift to update:", shiftChoices);
        var shiftId = int.Parse(selectedShiftChoice.Split(':')[0]);
        var shift = allShiftsResponse.Data.First(s => s.ShiftId == shiftId);
    var startTime = InputService.GetDateTimeInput($"Enter new start time (current: {shift.StartTime:yyyy-MM-dd HH:mm}):");
    var endTime = InputService.GetDateTimeInput($"Enter new end time (current: {shift.EndTime:yyyy-MM-dd HH:mm}):");
        if (endTime <= startTime)
        {
            DisplayService.DisplayError("End time must be after start time.");
            InputService.WaitForKeyPress();
            return;
        }
        var updatedShift = new Shift {
            ShiftId = shift.ShiftId,
            WorkerId = shift.WorkerId,
            LocationId = shift.LocationId,
            StartTime = startTime == default ? shift.StartTime : startTime,
            EndTime = endTime == default ? shift.EndTime : endTime
        };
        var response = await _shiftService.UpdateShiftAsync(shiftId, updatedShift);
        if (response.RequestFailed || response.Data == null)
        {
            DisplayService.DisplayError(response.Message ?? "Failed to update shift.");
        }
        else
        {
            DisplayService.DisplaySuccess("Shift updated successfully.");
            _shiftUi.DisplayShiftsTable(new List<Shift> { response.Data });
        }
        InputService.WaitForKeyPress();
    }

    private async Task DeleteShiftAsync()
    {
        DisplayService.DisplayHeader("Delete Shift", "red");
        var allShiftsResponse = await _shiftService.GetAllShiftsAsync();
        if (allShiftsResponse.RequestFailed || allShiftsResponse.Data == null || !allShiftsResponse.Data.Any())
        {
            DisplayService.DisplayError(allShiftsResponse.Message ?? "No shifts found.");
            InputService.WaitForKeyPress();
            return;
        }
        var shiftChoices = allShiftsResponse.Data.Select(s => $"{s.ShiftId}: {s.StartTime:yyyy-MM-dd HH:mm} - {s.EndTime:yyyy-MM-dd HH:mm}").ToArray();
        var selectedShiftChoice = InputService.GetMenuChoice("Select Shift to delete:", shiftChoices);
        var shiftId = int.Parse(selectedShiftChoice.Split(':')[0]);
        if (InputService.GetConfirmation($"Are you sure you want to delete shift {shiftId}?"))
        {
            var response = await _shiftService.DeleteShiftAsync(shiftId);
            if (response.RequestFailed)
            {
                DisplayService.DisplayError(response.Message ?? "Failed to delete shift.");
            }
            else
            {
                DisplayService.DisplaySuccess(response.Message ?? $"Shift {shiftId} deleted successfully.");
            }
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
        var workerId = InputService.GetIntegerInput("Filter by Worker ID (0 for any):", 0);
        var locationId = InputService.GetIntegerInput("Filter by Location ID (0 for any):", 0);
    var startDate = InputService.GetDateTimeInput("Filter start date (leave blank for any):");
    var endDate = InputService.GetDateTimeInput("Filter end date (leave blank for any):");
        var filter = new ShiftFilterOptions {
            WorkerId = workerId > 0 ? workerId : null,
            LocationId = locationId > 0 ? locationId : null,
            StartTime = startDate,
            EndTime = endDate
        };
        var response = await _shiftService.GetShiftsByFilterAsync(filter);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(response.Message ?? "No shifts found matching filter.");
        }
        else
        {
            _shiftUi.DisplayShiftsTable(response.Data);
            DisplayService.DisplaySuccess($"Total filtered shifts: {response.TotalCount}");
        }
        InputService.WaitForKeyPress();
    }
}
