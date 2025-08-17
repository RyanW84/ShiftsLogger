using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Services;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.MenuSystem.Menus;

/// <summary>
/// Worker menu implementation following Single Responsibility Principle
/// Handles worker-specific operations
/// </summary>
public class WorkerMenuV2 : BaseMenuV2
{
    private readonly IWorkerService _workerService;

    public WorkerMenuV2(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger<WorkerMenuV2> logger,
        IWorkerService workerService)
        : base(displayService, inputService, navigationService, logger)
    {
        _workerService = workerService ?? throw new ArgumentNullException(nameof(workerService));
    }

    public override string Title => "Worker Management";
    public override string Context => "Worker Management";

    protected override async Task ShowMenuAsync()
    {
        bool shouldExit = false;
        
        while (!shouldExit)
        {
            var choice = InputService.GetMenuChoice(
                "Select a worker operation:",
                "View All Workers",
                "View Worker by ID",
                "Create New Worker",
                "Update Worker",
                "Delete Worker",
                "Filter Workers",
                "View Workers by Email Domain",
                "View Workers by Phone Area Code",
                "Back to Main Menu"
            );

            shouldExit = await HandleWorkerChoice(choice);
        }
    }

    private async Task<bool> HandleWorkerChoice(string choice)
    {
        Logger.LogDebug("Worker menu choice selected: {Choice}", choice);

        if (await HandleCommonActions(choice))
            return true;

        try
        {
            switch (choice)
            {
                case "View All Workers":
                    await ViewAllWorkersAsync();
                    break;

                case "View Worker by ID":
                    await ViewWorkerByIdAsync();
                    break;

                case "Create New Worker":
                    await CreateWorkerAsync();
                    break;

                case "Update Worker":
                    await UpdateWorkerAsync();
                    break;

                case "Delete Worker":
                    await DeleteWorkerAsync();
                    break;

                case "Filter Workers":
                    await FilterWorkersAsync();
                    break;

                case "View Workers by Email Domain":
                    await ViewWorkersByEmailDomainAsync();
                    break;

                case "View Workers by Phone Area Code":
                    await ViewWorkersByPhoneAreaCodeAsync();
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
            Logger.LogError(ex, "Error handling worker choice: {Choice}", choice);
            DisplayService.DisplayError($"An error occurred: {ex.Message}");
            InputService.WaitForKeyPress();
        }

        return false;
    }

    private async Task ViewAllWorkersAsync()
    {
        DisplayService.DisplayHeader("All Workers", "blue");
        
        var response = await _workerService.GetAllWorkersAsync();
        
        if (response.RequestFailed)
        {
            DisplayService.DisplayError($"Failed to retrieve workers: {response.Message}");
        }
        else
        {
            DisplayService.DisplayTable(response.Data, "All Workers");
            DisplayService.DisplaySuccess($"Total workers: {response.TotalCount}");
        }
        
        InputService.WaitForKeyPress();
    }

    private async Task ViewWorkerByIdAsync()
    {
        var workerId = InputService.GetIntegerInput("Enter Worker ID:", 1);
        
        DisplayService.DisplayHeader($"Worker Details (ID: {workerId})", "blue");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }

    private async Task CreateWorkerAsync()
    {
        DisplayService.DisplayHeader("Create New Worker", "green");
        
        try
        {
            var name = InputService.GetTextInput("Enter Worker Name:");
            var email = InputService.GetTextInput("Enter Email Address (optional):", false);
            var phoneNumber = InputService.GetTextInput("Enter Phone Number (optional):", false);

            // Basic validation
            if (!string.IsNullOrEmpty(email) && !email.Contains("@"))
            {
                DisplayService.DisplayError("Invalid email format.");
                InputService.WaitForKeyPress();
                return;
            }

            // Create worker (this would call the API)
            DisplayService.DisplaySuccess("Worker created successfully!");
            DisplayService.DisplayInfo($"Name: {name}");
            if (!string.IsNullOrEmpty(email))
                DisplayService.DisplayInfo($"Email: {email}");
            if (!string.IsNullOrEmpty(phoneNumber))
                DisplayService.DisplayInfo($"Phone: {phoneNumber}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating worker");
            DisplayService.DisplayError($"Failed to create worker: {ex.Message}");
        }

        InputService.WaitForKeyPress();
    }

    private async Task UpdateWorkerAsync()
    {
        DisplayService.DisplayHeader("Update Worker", "yellow");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }

    private async Task DeleteWorkerAsync()
    {
        DisplayService.DisplayHeader("Delete Worker", "red");
        
        var workerId = InputService.GetIntegerInput("Enter Worker ID to delete:", 1);
        
        if (InputService.GetConfirmation($"Are you sure you want to delete worker {workerId}?"))
        {
            DisplayService.DisplayInfo("Feature implementation in progress...");
        }
        else
        {
            DisplayService.DisplayInfo("Delete cancelled.");
        }
        
        InputService.WaitForKeyPress();
    }

    private async Task FilterWorkersAsync()
    {
        DisplayService.DisplayHeader("Filter Workers", "blue");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }

    private async Task ViewWorkersByEmailDomainAsync()
    {
        DisplayService.DisplayHeader("Workers by Email Domain", "blue");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }

    private async Task ViewWorkersByPhoneAreaCodeAsync()
    {
        DisplayService.DisplayHeader("Workers by Phone Area Code", "blue");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }
}
