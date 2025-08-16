using System.Net;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
using Spectre.Console;

namespace ConsoleFrontEnd.Controller;

public class ShiftController
{
    private readonly ShiftService shiftService = new();
    private readonly MenuSystem.UserInterface userInterface = new();
    private readonly WorkerController workerController = new();

    private ShiftFilterOptions shiftFilterOptions = new()
    {
        ShiftId = null,
        WorkerId = null,
        StartTime = null,
        EndTime = null,
        StartDate = null,
        EndDate = null,
        LocationName = null,
        Search = null,
        SortBy = null,
        SortOrder = null
    };

    // Helpers
    public async Task<ApiResponseDto<Shift>> CheckShiftExists(int shiftId)
    {
        try
        {
            var response = await shiftService.GetShiftById(shiftId);

            while (response.ResponseCode is not HttpStatusCode.OK)
            {
                userInterface.DisplayErrorMessage(response.Message);
                Console.WriteLine();
                var exitSelection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Try again or exit?")
                        .AddChoices("Try Again", "Exit")
                );
                if (exitSelection is "Exit")
                    return new ApiResponseDto<Shift>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.NotFound,
                        Message = "User exited the operation.",
                        Data = null
                    };

                if (exitSelection is "Try Again")
                {
                    AnsiConsole.Markup("[Yellow]Please enter a correct ID: [/]");
                    shiftId = userInterface.GetShiftByIdUi();
                    response = await shiftService.GetShiftById(shiftId);
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for CheckShiftExists: {ex}");
            return new ApiResponseDto<Shift>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = $"Exception occurred: {ex.Message}",
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<int>> SelectShift(
        ShiftFilterOptions? shiftFilterOptions = null
    )
    {
        // Use default filter if none provided
        shiftFilterOptions ??= new ShiftFilterOptions();

        // Fetch shifts
        var response = await shiftService.GetAllShifts(shiftFilterOptions);

        if (response.Data == null || response.Data.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No shifts found.[/]");
            return new ApiResponseDto<int>
            {
                RequestFailed = true,
                ResponseCode = response.ResponseCode,
                Message = "No shifts available.",
                Data = 0
            };
        }

        // Prepare choices for the menu
        var choices = response.Data.Select(s => new
            { s.ShiftId, Display = $"{s.StartTime} {s.Location?.Name ?? "Unknown Location"}" }).ToList();

        // Show menu and get selection
        var selectedDisplay = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Select a shift[/]")
                .AddChoices(choices.Select(c => c.Display))
        );

        // Find the selected shift's ID
        var selected = choices.First(c => c.Display == selectedDisplay);

        return new ApiResponseDto<int>
        {
            RequestFailed = false,
            ResponseCode = response.ResponseCode,
            Message = "Shift selected.",
            Data = selected.ShiftId
        };
    }

    // CRUD
    public async Task CreateShift()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Create Shift[/]").RuleStyle("yellow").Centered()
            );
            var workerId = await workerController.SelectWorker();
            var shift = userInterface.CreateShiftUi(workerId.Data);
            var createdShift = await shiftService.CreateShift(shift);
            userInterface.ContinueAndClearScreen();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            userInterface.ContinueAndClearScreen();
        }
    }

    public async Task<ApiResponseDto<List<Shift>>> GetAllShifts()
    {
        try
        {
            shiftFilterOptions = userInterface.FilterShiftsUi();
            var response = await shiftService.GetAllShifts(shiftFilterOptions);

            if (response.Data is null || response.Data.Count == 0)
            {
                userInterface.DisplayErrorMessage(response.Message);
                return new ApiResponseDto<List<Shift>>
                {
                    RequestFailed = true,
                    ResponseCode = response.ResponseCode,
                    Message = "No shifts found.",
                    Data = new List<Shift>()
                };
            }

            userInterface.DisplaySuccessMessage(response.Message);
            userInterface.DisplayShiftsTable(response.Data);
            userInterface.ContinueAndClearScreen();
            return response;
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<List<Shift>>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = $"Exception occurred: {ex.Message}",
                Data = null
            };
        }
    }

    public async Task GetShiftById()
    {
        try
        {
            Console.Clear();

            AnsiConsole.Write(
                new Rule("[bold yellow]View Shift by ID[/]").RuleStyle("yellow").Centered()
            );
            var shiftId = await SelectShift();
            var shift = await shiftService.GetShiftById(shiftId.Data);

            if (shift.Data is not null)
            {
                userInterface.DisplayShiftsTable([shift.Data]);
                userInterface.ContinueAndClearScreen();
            }
            else
            {
                userInterface.DisplayErrorMessage(shift.Message);
                userInterface.ContinueAndClearScreen();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
    }

    public async Task UpdateShift()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Update Shift[/]").RuleStyle("yellow").Centered()
            );

            var shiftId = await SelectShift();
            var existingShift = await shiftService.GetShiftById(
                shiftId.Data
            );

            var updatedShift = userInterface.UpdateShiftUi(existingShift.Data);

            var updatedShiftResponse = await shiftService.UpdateShift(
                shiftId.Data,
                updatedShift
            );
            userInterface.DisplaySuccessMessage($"\n{updatedShiftResponse.Message}");
            userInterface.ContinueAndClearScreen();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Exception: {ex.Message}[/]");
        }
    }

    public async Task DeleteShift()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Delete Shift[/]").RuleStyle("yellow").Centered()
            );

            var shiftId = await SelectShift();
            var existingShift = await shiftService.GetShiftById(
                shiftId.Data
            );

            if (existingShift.Data is null)
            {
                userInterface.DisplayErrorMessage(existingShift.Message);
                userInterface.ContinueAndClearScreen();
                return;
            }

            var deletedShiftResponse = await shiftService.DeleteShift(
                existingShift.Data.ShiftId
            );

            if (deletedShiftResponse.RequestFailed)
                userInterface.DisplayErrorMessage(deletedShiftResponse.Message);
            else
                userInterface.DisplaySuccessMessage($"\n{deletedShiftResponse.Message}");

            userInterface.ContinueAndClearScreen();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try Pass failed in Shift Controller: Delete Shift {ex}");
            userInterface.ContinueAndClearScreen();
        }
    }

    public async Task<bool> IsWorkerAvailableForShift(int workerId, DateTime newShiftStart, DateTime newShiftEnd)
    {
        // Fetch all shifts for the worker
        var shiftFilterOptions = new ShiftFilterOptions
        {
            WorkerId = workerId
        };
        var response = await shiftService.GetAllShifts(shiftFilterOptions);

        if (response.Data == null)
            return true; // No shifts, so available

        // Check for overlap
        foreach (var shift in response.Data)
            if (shift.StartTime < newShiftEnd && newShiftStart < shift.EndTime)
                // Overlap detected
                return false;

        return true; // No overlap, worker is available
    }

    // NEW METHODS FOR SEPARATION OF CONCERNS

    // Method to get shift input for creation
    public async Task<(int workerId, Shift shift)> GetShiftInputAsync()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Create Shift - Input[/]").RuleStyle("yellow").Centered()
            );

            var workerIdResponse = await workerController.SelectWorker();
            if (workerIdResponse.RequestFailed) throw new InvalidOperationException(workerIdResponse.Message);

            var shift = userInterface.CreateShiftUi(workerIdResponse.Data);
            return (workerIdResponse.Data, shift);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get shift input: {ex.Message}", ex);
        }
    }

    // Method to create shift with provided data (no UI interaction)
    public async Task CreateShiftWithData(int workerId, Shift shift)
    {
        try
        {
            var response = await shiftService.CreateShift(shift);

            if (response.RequestFailed) throw new InvalidOperationException(response.Message);

            // Log success but don't display UI here - that's handled in the menu
            Console.WriteLine($"Shift created successfully: {response.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create shift: {ex.Message}", ex);
        }
    }

    // Method to make SelectShift async
    public async Task<ApiResponseDto<int>> SelectShiftAsync(ShiftFilterOptions? shiftFilterOptions = null)
    {
        return await Task.FromResult(await SelectShift(shiftFilterOptions));
    }

    // Method to get shift by ID with provided data (no UI interaction)
    public async Task GetShiftByIdWithData(int shiftId)
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]View Shift by ID - Results[/]").RuleStyle("yellow").Centered()
            );

            var shift = await shiftService.GetShiftById(shiftId);

            if (shift.Data is not null)
                userInterface.DisplayShiftsTable([shift.Data]);
            else
                throw new InvalidOperationException(shift.Message);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get shift by ID: {ex.Message}", ex);
        }
    }

    // Method to get update input for shift
    public async Task<(int shiftId, Shift updatedShift)> GetShiftUpdateInputAsync()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Update Shift - Input[/]").RuleStyle("yellow").Centered()
            );

            // First, select the shift to update
            var shiftIdResponse = await SelectShift();
            if (shiftIdResponse.RequestFailed) throw new InvalidOperationException(shiftIdResponse.Message);

            // Get the existing shift data
            var existingShift = await shiftService.GetShiftById(shiftIdResponse.Data);
            if (existingShift.Data is null) throw new InvalidOperationException(existingShift.Message);

            // Get the updated shift data from user
            var updatedShift = userInterface.UpdateShiftUi(existingShift.Data);

            return (shiftIdResponse.Data, updatedShift);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get shift update input: {ex.Message}", ex);
        }
    }

    // Method to update shift with provided data (no UI interaction)
    public async Task UpdateShiftWithData(int shiftId, Shift updatedShift)
    {
        try
        {
            var response = await shiftService.UpdateShift(shiftId, updatedShift);

            if (response.RequestFailed) throw new InvalidOperationException(response.Message);

            // Log success but don't display UI here - that's handled in the menu
            Console.WriteLine($"Shift updated successfully: {response.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update shift: {ex.Message}", ex);
        }
    }

    // Method to delete shift with provided data (no UI interaction)
    public async Task DeleteShiftWithData(int shiftId)
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Delete Shift - Processing[/]").RuleStyle("yellow").Centered()
            );

            // Get shift details first to verify it exists
            var existingShift = await shiftService.GetShiftById(shiftId);
            if (existingShift.Data is null) throw new InvalidOperationException(existingShift.Message);

            var response = await shiftService.DeleteShift(existingShift.Data.ShiftId);

            if (response.RequestFailed) throw new InvalidOperationException(response.Message);

            // Log success but don't display UI here - that's handled in the menu
            Console.WriteLine($"Shift deleted successfully: {response.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete shift: {ex.Message}", ex);
        }
    }

    public async Task<ShiftFilterOptions> GetShiftFilterInputAsync()
    {
        return await Task.FromResult(userInterface.FilterShiftsUi());
    }

    public async Task GetAllShiftsWithData(ShiftFilterOptions filterOptions)
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]View All Shifts - Results[/]").RuleStyle("yellow").Centered()
            );

            var response = await shiftService.GetAllShifts(filterOptions);

            if (response.Data is null || response.Data.Count == 0)
                throw new InvalidOperationException("No shifts found.");

            userInterface.DisplayShiftsTable(response.Data);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get all shifts: {ex.Message}", ex);
        }
    }
}