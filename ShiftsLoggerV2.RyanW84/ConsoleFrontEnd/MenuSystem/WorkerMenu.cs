using ConsoleFrontEnd.Controller;
using ConsoleFrontEnd.MenuSystem;
using Spectre.Console;

namespace ConsoleFrontEnd.UserInterface;

public class WorkerMenu : BaseMenu
{
    private static readonly WorkerController _workerController = new();

    public static async Task DisplayWorkerMenu()
    {
        bool continueLoop = true;

        while (continueLoop)
        {
            try
            {
                // Ensure clean state before displaying menu
                ClearConsoleState();
                DisplayHeader("Worker Management", "cyan");
                MenuHelpers.ShowBreadcrumb("Main Menu", "Worker Management");

                var choice = MenuHelpers.GetMenuChoice(
                    "Select an option:",
                    "Create New Worker",
                    "View All Workers",
                    "View Worker by ID",
                    "Update Worker",
                    "Delete Worker",
                    "Back to Main Menu"
                );

                continueLoop = await HandleWorkerMenuChoice(choice);
            }
            catch (Exception ex)
            {
                // Ensure clean state before showing error
                ClearConsoleState();
                DisplayErrorMessage($"An error occurred in Worker Menu: {ex.Message}");
                PauseForUserInput();
            }
        }
    }

    private static async Task<bool> HandleWorkerMenuChoice(string choice)
    {
        try
        {
            switch (choice)
            {
                case "Create New Worker":
                    await CreateWorkerWithFeedback();
                    break;
                case "View All Workers":
                    await ViewAllWorkersWithFeedback();
                    break;
                case "View Worker by ID":
                    await ViewWorkerByIdWithFeedback();
                    break;
                case "Update Worker":
                    await UpdateWorkerWithFeedback();
                    break;
                case "Delete Worker":
                    await DeleteWorkerWithFeedback();
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

    private static async Task CreateWorkerWithFeedback()
    {
        DisplayHeader("Create New Worker", "green");
        DisplayInfoMessage("Creating a new worker...");

        try
        {
            // Get all user input BEFORE starting the spinner
            var worker = await _workerController.GetWorkerInputAsync();
            
            await ShowLoadingSpinnerAsync("Processing worker creation...", async () =>
            {
                await _workerController.CreateWorkerWithData(worker);
            });

            // Ensure clean state before success message
            ClearConsoleState();
            DisplayHeader("Create New Worker", "green");
            DisplaySuccessMessage("Worker creation process completed.");
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("Create New Worker", "green");
            DisplayErrorMessage($"Worker creation failed: {ex.Message}");
        }
        
        PauseForUserInput();
    }

    private static async Task ViewAllWorkersWithFeedback()
    {
        DisplayHeader("View All Workers", "blue");
        
        try
        {
            // Get filter options BEFORE starting the spinner
            var filterOptions = await _workerController.GetWorkerFilterInputAsync();

            await ShowLoadingSpinnerAsync("Loading workers...", async () =>
            {
                await _workerController.GetAllWorkersWithData(filterOptions);
            });

            // Success state is handled by the controller, no need to clear
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("View All Workers", "blue");
            DisplayErrorMessage($"Failed to load workers: {ex.Message}");
        }

        PauseForUserInput();
    }

    private static async Task ViewWorkerByIdWithFeedback()
    {
        DisplayHeader("View Worker by ID", "blue");
        
        try
        {
            // Get worker selection BEFORE starting the spinner
            var selectedWorkerId = await _workerController.SelectWorkerAsync();
            if (selectedWorkerId.RequestFailed)
            {
                DisplayErrorMessage(selectedWorkerId.Message);
                PauseForUserInput();
                return;
            }

            await ShowLoadingSpinnerAsync("Loading worker details...", async () =>
            {
                await _workerController.GetWorkerByIdWithData(selectedWorkerId.Data);
            });

            // Success state is handled by the controller, no need to clear
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("View Worker by ID", "blue");
            DisplayErrorMessage($"Failed to load worker details: {ex.Message}");
        }

        PauseForUserInput();
    }

    private static async Task UpdateWorkerWithFeedback()
    {
        DisplayHeader("Update Worker", "orange3");

        if (!ConfirmAction("update a worker"))
        {
            DisplayInfoMessage("Update operation cancelled.");
            PauseForUserInput();
            return;
        }

        try
        {
            // Get all user input BEFORE starting the spinner
            var (workerId, updatedWorker) = await _workerController.GetWorkerUpdateInputAsync();
            
            await ShowLoadingSpinnerAsync("Processing worker update...", async () =>
            {
                await _workerController.UpdateWorkerWithData(workerId, updatedWorker);
            });

            // Ensure clean state before success message
            ClearConsoleState();
            DisplayHeader("Update Worker", "orange3");
            DisplaySuccessMessage("Worker update process completed.");
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("Update Worker", "orange3");
            DisplayErrorMessage($"Worker update failed: {ex.Message}");
        }

        PauseForUserInput();
    }

    private static async Task DeleteWorkerWithFeedback()
    {
        DisplayHeader("Delete Worker", "red");
        DisplayInfoMessage("Warning: This action cannot be undone!");

        if (!ConfirmAction("delete a worker"))
        {
            DisplayInfoMessage("Delete operation cancelled.");
            PauseForUserInput();
            return;
        }

        try
        {
            // Get worker selection BEFORE starting the spinner
            var selectedWorkerId = await _workerController.SelectWorkerAsync();
            if (selectedWorkerId.RequestFailed)
            {
                DisplayErrorMessage(selectedWorkerId.Message);
                PauseForUserInput();
                return;
            }

            await ShowLoadingSpinnerAsync("Processing worker deletion...", async () =>
            {
                await _workerController.DeleteWorkerWithData(selectedWorkerId.Data);
            });

            // Ensure clean state before success message
            ClearConsoleState();
            DisplayHeader("Delete Worker", "red");
            DisplaySuccessMessage("Worker deletion process completed.");
        }
        catch (Exception ex)
        {
            // Ensure clean state before error message
            ClearConsoleState();
            DisplayHeader("Delete Worker", "red");
            DisplayErrorMessage($"Worker deletion failed: {ex.Message}");
        }

        PauseForUserInput();
    }
}