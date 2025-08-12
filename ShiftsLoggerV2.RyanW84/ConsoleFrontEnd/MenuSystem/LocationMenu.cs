using ConsoleFrontEnd.Controller;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public class LocationMenu : BaseMenu
{
    private static readonly LocationController _locationController = new();

    public static async Task DisplayLocationMenu()
    {
        bool continueLoop = true;

        while (continueLoop)
        {
            try
            {
                // Ensure clean state before displaying menu
                ClearConsoleState();
                DisplayHeader("Location Management", "magenta");
                MenuHelpers.ShowBreadcrumb("Main Menu", "Location Management");

                var choice = MenuHelpers.GetMenuChoice(
                    "Select an option:",
                    "Create New Location",
                    "View All Locations",
                    "View Location by ID",
                    "Update Location",
                    "Delete Location",
                    "Back to Main Menu"
                );

                continueLoop = await HandleLocationMenuChoice(choice);
            }
            catch (Exception ex)
            {
                // Ensure clean state before showing error
                ClearConsoleState();
                DisplayErrorMessage($"An error occurred in Location Menu: {ex.Message}");
                PauseForUserInput();
            }
        }
    }

    private static async Task<bool> HandleLocationMenuChoice(string choice)
    {
        try
        {
            switch (choice)
            {
                case "Create New Location":
                    await CreateLocationWithFeedback();
                    break;
                case "View All Locations":
                    await ViewAllLocationsWithFeedback();
                    break;
                case "View Location by ID":
                    await ViewLocationByIdWithFeedback();
                    break;
                case "Update Location":
                    await UpdateLocationWithFeedback();
                    break;
                case "Delete Location":
                    await DeleteLocationWithFeedback();
                    break;
                case "Back to Main Menu":
                    MainMenu.ReturnToMainMenu();
                    return false;
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

        return true;
    }

    private static async Task CreateLocationWithFeedback()
    {
        DisplayHeader("Create New Location", "green");
        DisplayInfoMessage("Creating a new location...");

        try
        {
            // Get all user input BEFORE starting the spinner
            var location = await _locationController.GetLocationInputAsync();
            
            await ShowLoadingSpinnerAsync("Processing location creation...", async () =>
            {
                await _locationController.CreateLocationWithData(location);
            });

            // Ensure clean state before success message
            ClearConsoleState();
            DisplayHeader("Create New Location", "green");
            DisplaySuccessMessage("Location creation process completed.");
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("Create New Location", "green");
            DisplayErrorMessage($"Location creation failed: {ex.Message}");
        }

        PauseForUserInput();
    }

    private static async Task ViewAllLocationsWithFeedback()
    {
        DisplayHeader("View All Locations", "blue");

        try
        {
            // Get filter options BEFORE starting the spinner
            var filterOptions = await _locationController.GetLocationFilterInputAsync();

            await ShowLoadingSpinnerAsync("Loading locations...", async () =>
            {
                await _locationController.GetAllLocationsWithData(filterOptions);
            });

            // Success state is handled by the controller, no need to clear
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("View All Locations", "blue");
            DisplayErrorMessage($"Failed to load locations: {ex.Message}");
        }

        PauseForUserInput();
    }

    private static async Task ViewLocationByIdWithFeedback()
    {
        DisplayHeader("View Location by ID", "blue");
        
        try
        {
            // Get location selection BEFORE starting the spinner
            var selectedLocationId = await _locationController.SelectLocationAsync();
            if (selectedLocationId.RequestFailed)
            {
                DisplayErrorMessage(selectedLocationId.Message);
                PauseForUserInput();
                return;
            }

            await ShowLoadingSpinnerAsync("Loading location details...", async () =>
            {
                await _locationController.GetLocationByIdWithData(selectedLocationId.Data);
            });

            // Success state is handled by the controller, no need to clear
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("View Location by ID", "blue");
            DisplayErrorMessage($"Failed to load location details: {ex.Message}");
        }

        PauseForUserInput();
    }

    private static async Task UpdateLocationWithFeedback()
    {
        DisplayHeader("Update Location", "orange3");

        if (!ConfirmAction("update a location"))
        {
            DisplayInfoMessage("Update operation cancelled.");
            PauseForUserInput();
            return;
        }

        try
        {
            // Get all user input BEFORE starting the spinner
            var (locationId, updatedLocation) = await _locationController.GetLocationUpdateInputAsync();
            
            await ShowLoadingSpinnerAsync("Processing location update...", async () =>
            {
                await _locationController.UpdateLocationWithData(locationId, updatedLocation);
            });

            // Ensure clean state before success message
            ClearConsoleState();
            DisplayHeader("Update Location", "orange3");
            DisplaySuccessMessage("Location update process completed.");
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("Update Location", "orange3");
            DisplayErrorMessage($"Location update failed: {ex.Message}");
        }

        PauseForUserInput();
    }

    private static async Task DeleteLocationWithFeedback()
    {
        DisplayHeader("Delete Location", "red");
        DisplayInfoMessage("Warning: This action cannot be undone!");
        DisplayInfoMessage("Note: Delete Location functionality may not be fully implemented.");

        if (!ConfirmAction("delete a location"))
        {
            DisplayInfoMessage("Delete operation cancelled.");
            PauseForUserInput();
            return;
        }

        try
        {
            // Get location selection BEFORE starting the spinner
            var selectedLocationId = await _locationController.SelectLocationAsync();
            if (selectedLocationId.RequestFailed)
            {
                DisplayErrorMessage(selectedLocationId.Message);
                PauseForUserInput();
                return;
            }

            await ShowLoadingSpinnerAsync("Processing location deletion...", async () =>
            {
                await _locationController.DeleteLocationWithData(selectedLocationId.Data);
            });

            // Ensure clean state before success message
            ClearConsoleState();
            DisplayHeader("Delete Location", "red");
            DisplaySuccessMessage("Location deletion process completed.");
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("Delete Location", "red");
            DisplayErrorMessage($"Location deletion failed: {ex.Message}");
        }

        PauseForUserInput();
    }
}
