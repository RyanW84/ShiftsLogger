using ConsoleFrontEnd.Controller;
using ConsoleFrontEnd.Models.FilterOptions;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public class ShiftMenu : BaseMenu
{
    private static readonly ShiftController _shiftController = new();
    
    public static async Task DisplayShiftMenu()
    {
        bool continueLoop = true;
        
        while (continueLoop)
        {
            try
            {
                // Ensure clean state before displaying menu
                ClearConsoleState();
                DisplayHeader("Shift Management", "yellow");
                MenuHelpers.ShowBreadcrumb("Main Menu", "Shift Management");

                var choice = MenuHelpers.GetMenuChoice(
                    "Select an option:",
                    "Create New Shift",
                    "View All Shifts",
                    "View Shift by ID",
                    "Update Shift",
                    "Delete Shift",
                    "Back to Main Menu"
                );

                continueLoop = await HandleShiftMenuChoice(choice);
            }
            catch (Exception ex)
            {
                // Ensure clean state before showing error
                ClearConsoleState();
                DisplayErrorMessage($"An error occurred in Shift Menu: {ex.Message}");
                PauseForUserInput();
            }
        }
    }

    private static async Task<bool> HandleShiftMenuChoice(string choice)
    {
        try
        {
            switch (choice)
            {
                case "Create New Shift":
                    await CreateShiftWithFeedback();
                    break;
                case "View All Shifts":
                    await ViewAllShiftsWithFeedback();
                    break;
                case "View Shift by ID":
                    await ViewShiftByIdWithFeedback();
                    break;
                case "Update Shift":
                    await UpdateShiftWithFeedback();
                    break;
                case "Delete Shift":
                    await DeleteShiftWithFeedback();
                    break;
                case "Back to Main Menu":
                    MainMenu.ReturnToMainMenu();
                    return false; // Exit the loop
                default:
                    DisplayErrorMessage("Invalid choice, please try again.");
                    PauseForUserInput();
                    break;
            }
        }
        catch (Exception ex)
        {
            // Ensure clean state before showing error
            ClearConsoleState();
            DisplayErrorMessage($"Operation failed: {ex.Message}");
            PauseForUserInput();
        }
        
        return true; // Continue the loop
    }

    private static async Task CreateShiftWithFeedback()
    {
        DisplayHeader("Create New Shift", "green");
        DisplayInfoMessage("Creating a new shift...");

        try
        {
            // Get all user input BEFORE starting the spinner
            var (workerId, shift) = await _shiftController.GetShiftInputAsync();
            
            await ShowLoadingSpinnerAsync("Processing shift creation...", async () =>
            {
                await _shiftController.CreateShiftWithData(workerId, shift);
            });
            
            // Ensure clean state before success message
            ClearConsoleState();
            DisplayHeader("Create New Shift", "green");
            DisplaySuccessMessage("Shift creation process completed.");
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("Create New Shift", "green");
            DisplayErrorMessage($"Shift creation failed: {ex.Message}");
        }
        
        PauseForUserInput();
    }

    private static async Task ViewAllShiftsWithFeedback()
    {
        DisplayHeader("View All Shifts", "blue");
        
        try
        {
            // Get filter options BEFORE starting the spinner
            var filterOptions = await _shiftController.GetShiftFilterInputAsync();
            
            await ShowLoadingSpinnerAsync("Loading shifts...", async () =>
            {
                await _shiftController.GetAllShiftsWithData(filterOptions);
            });
            
            // Success state is handled by the controller, no need to clear
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("View All Shifts", "blue");
            DisplayErrorMessage($"Failed to load shifts: {ex.Message}");
        }
        
        PauseForUserInput();
    }

    private static async Task ViewShiftByIdWithFeedback()
    {
        DisplayHeader("View Shift by ID", "blue");
        
        try
        {
            // Get shift selection BEFORE starting the spinner
            var selectedShiftId = await _shiftController.SelectShiftAsync();
            if (selectedShiftId.RequestFailed)
            {
                DisplayErrorMessage(selectedShiftId.Message);
                PauseForUserInput();
                return;
            }
            
            await ShowLoadingSpinnerAsync("Loading shift details...", async () =>
            {
                await _shiftController.GetShiftByIdWithData(selectedShiftId.Data);
            });
            
            // Success state is handled by the controller, no need to clear
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("View Shift by ID", "blue");
            DisplayErrorMessage($"Failed to load shift details: {ex.Message}");
        }
        
        PauseForUserInput();
    }

    private static async Task UpdateShiftWithFeedback()
    {
        DisplayHeader("Update Shift", "orange3");
        
        if (!ConfirmAction("update a shift"))
        {
            DisplayInfoMessage("Update operation cancelled.");
            PauseForUserInput();
            return;
        }

        try
        {
            // Get all user input BEFORE starting the spinner
            var (shiftId, updatedShift) = await _shiftController.GetShiftUpdateInputAsync();
            
            await ShowLoadingSpinnerAsync("Processing shift update...", async () =>
            {
                await _shiftController.UpdateShiftWithData(shiftId, updatedShift);
            });
            
            // Ensure clean state before success message
            ClearConsoleState();
            DisplayHeader("Update Shift", "orange3");
            DisplaySuccessMessage("Shift update process completed.");
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("Update Shift", "orange3");
            DisplayErrorMessage($"Shift update failed: {ex.Message}");
        }
        
        PauseForUserInput();
    }

    private static async Task DeleteShiftWithFeedback()
    {
        DisplayHeader("Delete Shift", "red");
        DisplayInfoMessage("Warning: This action cannot be undone!");
        
        if (!ConfirmAction("delete a shift"))
        {
            DisplayInfoMessage("Delete operation cancelled.");
            PauseForUserInput();
            return;
        }

        try
        {
            // Get shift selection BEFORE starting the spinner
            var selectedShiftId = await _shiftController.SelectShiftAsync();
            if (selectedShiftId.RequestFailed)
            {
                DisplayErrorMessage(selectedShiftId.Message);
                PauseForUserInput();
                return;
            }
            
            await ShowLoadingSpinnerAsync("Processing shift deletion...", async () =>
            {
                await _shiftController.DeleteShiftWithData(selectedShiftId.Data);
            });
            
            // Ensure clean state before success message
            ClearConsoleState();
            DisplayHeader("Delete Shift", "red");
            DisplaySuccessMessage("Shift deletion process completed.");
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("Delete Shift", "red");
            DisplayErrorMessage($"Shift deletion failed: {ex.Message}");
        }
        
        PauseForUserInput();
    }
}
