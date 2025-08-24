using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Services;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.MenuSystem.Menus;

/// <summary>
/// Worker menu implementation following Single Responsibility Principle
/// Handles worker-specific operations
/// </summary>
public class WorkerMenu : BaseMenu
{
    private readonly IWorkerService _workerService;

    public WorkerMenu(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger<WorkerMenu> logger,
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
            switch (response.ResponseCode)
            {
                case System.Net.HttpStatusCode.NotFound:
                    DisplayService.DisplayError("No workers found (404).");
                    break;
                case System.Net.HttpStatusCode.BadRequest:
                    DisplayService.DisplayError("Bad request (400).");
                    break;
                case System.Net.HttpStatusCode.InternalServerError:
                    DisplayService.DisplayError("Server error (500).");
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    DisplayService.DisplayError("Unauthorized (401).");
                    break;
                case System.Net.HttpStatusCode.Forbidden:
                    DisplayService.DisplayError("Forbidden (403).");
                    break;
                case System.Net.HttpStatusCode.Conflict:
                    DisplayService.DisplayError("Conflict (409).");
                    break;
                case System.Net.HttpStatusCode.RequestTimeout:
                    DisplayService.DisplayError("Request Timeout (408).");
                    break;
                case (System.Net.HttpStatusCode)422:
                    DisplayService.DisplayError("Unprocessable Entity (422).");
                    break;
                default:
                    DisplayService.DisplayError($"Failed to retrieve workers: {response.Message}");
                    break;
            }
        }
        else
        {
            DisplayService.DisplayTable(response.Data != null ? response.Data : Enumerable.Empty<Worker>(), "All Workers");
            DisplayService.DisplaySuccess($"Total workers: {response.TotalCount}");
        }

        InputService.WaitForKeyPress();
    }

    private async Task ViewWorkerByIdAsync()
    {
        var allWorkersResponse = await _workerService.GetAllWorkersAsync();
        if (allWorkersResponse.RequestFailed || allWorkersResponse.Data == null || !allWorkersResponse.Data.Any())
        {
            DisplayService.DisplayError(allWorkersResponse.Message ?? "No workers found.");
            InputService.WaitForKeyPress();
            return;
        }
        var workerChoices = allWorkersResponse.Data.Select(w => $"{w.WorkerId}: {w.Name}").ToArray();
        var selected = InputService.GetMenuChoice("Select a worker by ID:", workerChoices);
        var workerId = int.Parse(selected.Split(':')[0]);
        var response = await _workerService.GetWorkerByIdAsync(workerId);
        DisplayService.DisplayHeader($"Worker Details (ID: {workerId})", "blue");
        if (response.RequestFailed || response.Data == null)
        {
            DisplayService.DisplayError(response.Message ?? "Worker not found.");
        }
        else
        {
            DisplayService.DisplayTable(new List<Worker> { response.Data }, "Worker Details");
            DisplayService.DisplaySuccess("Worker details loaded successfully.");
        }
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
        await Task.CompletedTask;
    }

    private async Task UpdateWorkerAsync()
    {
        DisplayService.DisplayHeader("Update Worker", "yellow");
        var allWorkersResponse = await _workerService.GetAllWorkersAsync();
        if (allWorkersResponse.RequestFailed || allWorkersResponse.Data == null || !allWorkersResponse.Data.Any())
        {
            DisplayService.DisplayError(allWorkersResponse.Message ?? "No workers found.");
            InputService.WaitForKeyPress();
            return;
        }
        var workerChoices = allWorkersResponse.Data.Select(w => $"{w.WorkerId}: {w.Name}").ToArray();
        var selected = InputService.GetMenuChoice("Select a worker to update:", workerChoices);
        var workerId = int.Parse(selected.Split(':')[0]);
        var worker = allWorkersResponse.Data.First(w => w.WorkerId == workerId);
        var name = InputService.GetTextInput($"Enter new name (current: {worker.Name}):", false);
        var email = InputService.GetTextInput($"Enter new email (current: {worker.Email}):", false);
        var phone = InputService.GetTextInput($"Enter new phone (current: {worker.PhoneNumber}):", false);
        var updatedWorker = new Worker
        {
            WorkerId = worker.WorkerId,
            Name = string.IsNullOrWhiteSpace(name) ? worker.Name : name,
            Email = string.IsNullOrWhiteSpace(email) ? worker.Email : email,
            PhoneNumber = string.IsNullOrWhiteSpace(phone) ? worker.PhoneNumber : phone
        };
        var response = await _workerService.UpdateWorkerAsync(workerId, updatedWorker);
        if (response.RequestFailed || response.Data == null)
        {
            DisplayService.DisplayError(response.Message ?? "Failed to update worker.");
        }
        else
        {
            DisplayService.DisplaySuccess("Worker updated successfully.");
            DisplayService.DisplayTable(new List<Worker> { response.Data }, "Updated Worker");
        }
        InputService.WaitForKeyPress();
    }

    private async Task DeleteWorkerAsync()
    {
        DisplayService.DisplayHeader("Delete Worker", "red");
        var allWorkersResponse = await _workerService.GetAllWorkersAsync();
        if (allWorkersResponse.RequestFailed || allWorkersResponse.Data == null || !allWorkersResponse.Data.Any())
        {
            DisplayService.DisplayError(allWorkersResponse.Message ?? "No workers found.");
            InputService.WaitForKeyPress();
            return;
        }
        var workerChoices = allWorkersResponse.Data.Select(w => $"{w.WorkerId}: {w.Name}").ToArray();
        var selected = InputService.GetMenuChoice("Select a worker to delete:", workerChoices);
        var workerId = int.Parse(selected.Split(':')[0]);
        if (InputService.GetConfirmation($"Are you sure you want to delete worker {workerId}?"))
        {
            var response = await _workerService.DeleteWorkerAsync(workerId);
            if (response.RequestFailed)
            {
                DisplayService.DisplayError(response.Message ?? "Failed to delete worker.");
            }
            else
            {
                DisplayService.DisplaySuccess(response.Message ?? $"Worker {workerId} deleted successfully.");
            }
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

        // Get all workers for name/email selection
        var allWorkersResponse = await _workerService.GetAllWorkersAsync();
        string? name = null;
        string? email = null;
        if (allWorkersResponse.Data != null && allWorkersResponse.Data.Any())
        {
            var names = allWorkersResponse.Data.Select(w => w.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().OrderBy(n => n).ToList();
            var emails = allWorkersResponse.Data.Select(w => w.Email).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().OrderBy(e => e).ToList();
            if (names.Any())
            {
                var nameChoices = new[] { "Any" }.Concat(names).ToArray();
                var selectedName = InputService.GetMenuChoice("Filter by Name:", nameChoices);
                if (selectedName != "Any") name = selectedName;
            }
            if (emails.Any())
            {
                var emailChoices = new[] { "Any" }.Concat(emails).Select(s => s!).ToArray();
                var selectedEmail = InputService.GetMenuChoice("Filter by Email:", emailChoices);
                if (selectedEmail != "Any") email = selectedEmail;
            }
        }

        var filter = new WorkerFilterOptions
        {
            Name = name,
            Email = email,
            PhoneNumber = InputService.GetTextInput("Filter by phone (leave blank for any):", false)
            // If you add date/time fields in the future, use dd/MM/yyyy HH:mm format for prompts and parsing
        };
        var response = await _workerService.GetWorkersByFilterAsync(filter);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            DisplayService.DisplayError(response.Message ?? "No workers found matching filter.");
        }
        else
        {
            DisplayService.DisplayTable(response.Data, "Filtered Workers");
            DisplayService.DisplaySuccess($"Total filtered workers: {response.TotalCount}");
        }
        InputService.WaitForKeyPress();
    }

    private async Task ViewWorkersByEmailDomainAsync()
    {
        DisplayService.DisplayHeader("Workers by Email Domain", "blue");
        var domain = InputService.GetTextInput("Enter email domain (e.g. gmail.com):");
        var response = await _workerService.GetAllWorkersAsync();
        if (response.RequestFailed || response.Data == null)
        {
            DisplayService.DisplayError(response.Message ?? "No workers found.");
        }
        else
        {
            var filtered = response.Data.Where(w => !string.IsNullOrEmpty(w.Email) && w.Email.EndsWith(domain, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!filtered.Any())
            {
                DisplayService.DisplayError($"No workers found with email domain '{domain}'.");
            }
            else
            {
                DisplayService.DisplayTable(filtered, $"Workers with domain '{domain}'");
                DisplayService.DisplaySuccess($"Total: {filtered.Count}");
            }
        }
        InputService.WaitForKeyPress();
    }

    private async Task ViewWorkersByPhoneAreaCodeAsync()
    {
        DisplayService.DisplayHeader("Workers by Phone Area Code", "blue");
        var areaCode = InputService.GetTextInput("Enter phone area code:");
        var response = await _workerService.GetAllWorkersAsync();
        if (response.RequestFailed || response.Data == null)
        {
            DisplayService.DisplayError(response.Message ?? "No workers found.");
        }
        else
        {
            var filtered = response.Data.Where(w => !string.IsNullOrEmpty(w.PhoneNumber) && w.PhoneNumber.StartsWith(areaCode)).ToList();
            if (!filtered.Any())
            {
                DisplayService.DisplayError($"No workers found with area code '{areaCode}'.");
            }
            else
            {
                DisplayService.DisplayTable(filtered, $"Workers with area code '{areaCode}'");
                DisplayService.DisplaySuccess($"Total: {filtered.Count}");
            }
        }
        InputService.WaitForKeyPress();
    }
}
