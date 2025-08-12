using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
using Spectre.Console;

namespace ConsoleFrontEnd.Controller;

public class WorkerController
{
    private readonly MenuSystem.UserInterface userInterface = new();
    private readonly WorkerService workerService = new();
    private WorkerFilterOptions workerFilterOptions = new()
    {
        WorkerId = null,
        Name = null,
        PhoneNumber = null,
        Email = null,
        Search = null,
        SortBy = "Name", // Default sorting by name
        SortOrder = "asc", // Default sorting order is ascending
    };

    // Helpers
    public async Task<ApiResponseDto<Worker>> CheckWorkerExists(int workerId)
    {
        try
        {
            var response = await workerService.GetWorkerById(workerId);

            while (response.ResponseCode is not System.Net.HttpStatusCode.OK)
            {
                userInterface.DisplayErrorMessage(response.Message);
                Console.WriteLine();
                var exitSelection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Try again or exit?")
                        .AddChoices(new[] { "Try Again", "Exit" })
                );
                if (exitSelection is "Exit")
                {
                    return new ApiResponseDto<Worker>
                    {
                        RequestFailed = true,
                        ResponseCode = System.Net.HttpStatusCode.NotFound,
                        Message = "User exited the operation.",
                        Data = null,
                    };
                }
                else if (exitSelection is "Try Again")
                {
                    AnsiConsole.Markup("[Yellow]Please enter a correct ID: [/]");
                    workerId = userInterface.GetWorkerByIdUi();
                    response = await workerService.GetWorkerById(workerId);
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for CheckWorkerExists: {ex}");
            return new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Exception occurred: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<ApiResponseDto<int>> SelectWorker(
        WorkerFilterOptions? workerFilterOptions = null
    )
    {
        // Use default filter if none provided
        workerFilterOptions ??= new WorkerFilterOptions();

        // Fetch workers
        var response = await workerService.GetAllWorkers(workerFilterOptions);

        if (response.Data == null || response.Data.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No workers found.[/]");
            return new ApiResponseDto<int>
            {
                RequestFailed = true,
                ResponseCode = response.ResponseCode,
                Message = "No workers available.",
                Data = 0,
            };
        }

        // Prepare choices for the menu
        var choices = response.Data.Select(w => new { w.WorkerId, Display = $"{w.Name}" }).ToList();

        // Show menu and get selection
        var selectedDisplay = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Select a worker[/]")
                .AddChoices(choices.Select(c => c.Display))
        );

        // Find the selected worker's ID
        var selected = choices.First(c => c.Display == selectedDisplay);

        return new ApiResponseDto<int>
        {
            RequestFailed = false,
            ResponseCode = response.ResponseCode,
            Message = "Worker selected.",
            Data = selected.WorkerId,
        };
    }

    // CRUD
    public async Task CreateWorker()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Create Worker[/]").RuleStyle("yellow").Centered()
            );
            var worker = userInterface.CreateWorkerUi();
            var createdWorker = await workerService.CreateWorker(worker);
            userInterface.ContinueAndClearScreen();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            userInterface.ContinueAndClearScreen();
        }
    }

    public async Task GetAllWorkers()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]View All Workers[/]").RuleStyle("yellow").Centered()
            );

            var filterOptions = userInterface.FilterWorkersUi();

            workerFilterOptions = filterOptions;
            var response = await workerService.GetAllWorkers(workerFilterOptions);

            if (response.Data is null)
            {
                AnsiConsole.MarkupLine("[red]No workers found.[/]");
                userInterface.ContinueAndClearScreen();
            }
            else
                userInterface.DisplayWorkersTable(response.Data);
            userInterface.ContinueAndClearScreen();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            userInterface.ContinueAndClearScreen();
        }
    }

    public async Task GetWorkerById()
    {
        try
        {
            Console.Clear();

            AnsiConsole.Write(
                new Rule("[bold yellow]View Worker by ID[/]").RuleStyle("yellow").Centered()
            );
            ApiResponseDto<int>? workerId = await SelectWorker();
            ApiResponseDto<Worker> worker = await workerService.GetWorkerById(workerId.Data);

            if (worker.Data is not null)
            {
                userInterface.DisplayWorkersTable([worker.Data]);
                userInterface.ContinueAndClearScreen();
            }
            else
            {
                userInterface.DisplayErrorMessage(worker.Message);
                userInterface.ContinueAndClearScreen();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
    }

    public async Task UpdateWorker()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Update Worker[/]").RuleStyle("yellow").Centered()
            );

            ApiResponseDto<int>? workerId = await SelectWorker();
            ApiResponseDto<Worker> existingWorker = await workerService.GetWorkerById(
                workerId.Data
            );

            var updatedWorker = userInterface.UpdateWorkerUi(existingWorker.Data);

            var updatedWorkerResponse = await workerService.UpdateWorker(
                workerId.Data,
                updatedWorker
            );
            userInterface.DisplaySuccessMessage($"\n{updatedWorkerResponse.Message}");
            userInterface.ContinueAndClearScreen();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Exception: {ex.Message}[/]");
        }
    }

    public async Task DeleteWorker()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Delete Worker[/]").RuleStyle("yellow").Centered()
            );

            ApiResponseDto<int>? workerId = await SelectWorker();
            ApiResponseDto<Worker> existingWorker = await workerService.GetWorkerById(
                workerId.Data
            );

            if (existingWorker.Data is null)
            {
                userInterface.DisplayErrorMessage(existingWorker.Message);
                userInterface.ContinueAndClearScreen();
                return;
            }

            var deletedWorkerResponse = await workerService.DeleteWorker(
                existingWorker.Data.WorkerId
            );

            if (deletedWorkerResponse.RequestFailed)
            {
                userInterface.DisplayErrorMessage(deletedWorkerResponse.Message);
            }
            else
            {
                userInterface.DisplaySuccessMessage($"\n{deletedWorkerResponse.Message}");
            }

            userInterface.ContinueAndClearScreen();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try Pass failed in Worker Controller: Delete Worker {ex}");
            userInterface.ContinueAndClearScreen();
        }
    }

    // New Methods
    // Method to get worker input for creation
    public async Task<Worker> GetWorkerInputAsync()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Create Worker - Input[/]").RuleStyle("yellow").Centered()
            );
            
            var worker = userInterface.CreateWorkerUi();
            return worker;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get worker input: {ex.Message}", ex);
        }
    }

    // Method to create worker with provided data (no UI interaction)
    public async Task CreateWorkerWithData(Worker worker)
    {
        try
        {
            var response = await workerService.CreateWorker(worker);
            
            if (response.RequestFailed)
            {
                throw new InvalidOperationException(response.Message);
            }
            
            // Log success but don't display UI here - that's handled in the menu
            Console.WriteLine($"Worker created successfully: {response.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create worker: {ex.Message}", ex);
        }
    }

    // Method to make SelectWorker async (your existing method is already good, just make it async)
    public async Task<ApiResponseDto<int>> SelectWorkerAsync(WorkerFilterOptions? workerFilterOptions = null)
    {
        // This method can remain the same as your existing SelectWorker method
        // Just make sure it's async and rename it
        return await Task.FromResult(await SelectWorker(workerFilterOptions));
    }

    // Method to get worker by ID with provided data (no UI interaction)
    public async Task GetWorkerByIdWithData(int workerId)
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]View Worker by ID - Results[/]").RuleStyle("yellow").Centered()
            );
            
            var worker = await workerService.GetWorkerById(workerId);

            if (worker.Data is not null)
            {
                userInterface.DisplayWorkersTable([worker.Data]);
            }
            else
            {
                throw new InvalidOperationException(worker.Message);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get worker by ID: {ex.Message}", ex);
        }
    }

    // Method to get update input for worker
    public async Task<(int workerId, Worker updatedWorker)> GetWorkerUpdateInputAsync()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Update Worker - Input[/]").RuleStyle("yellow").Centered()
            );

            // First, select the worker to update
            var workerIdResponse = await SelectWorker();
            if (workerIdResponse.RequestFailed)
            {
                throw new InvalidOperationException(workerIdResponse.Message);
            }

            // Get the existing worker data
            var existingWorker = await workerService.GetWorkerById(workerIdResponse.Data);
            if (existingWorker.Data is null)
            {
                throw new InvalidOperationException(existingWorker.Message);
            }

            // Get the updated worker data from user
            var updatedWorker = userInterface.UpdateWorkerUi(existingWorker.Data);

            return (workerIdResponse.Data, updatedWorker);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get worker update input: {ex.Message}", ex);
        }
    }

    // Method to update worker with provided data (no UI interaction)
    public async Task UpdateWorkerWithData(int workerId, Worker updatedWorker)
    {
        try
        {
            var response = await workerService.UpdateWorker(workerId, updatedWorker);
            
            if (response.RequestFailed)
            {
                throw new InvalidOperationException(response.Message);
            }
            
            // Log success but don't display UI here - that's handled in the menu
            Console.WriteLine($"Worker updated successfully: {response.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update worker: {ex.Message}", ex);
        }
    }

    // Method to delete worker with provided data (no UI interaction)
    public async Task DeleteWorkerWithData(int workerId)
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Delete Worker - Processing[/]").RuleStyle("yellow").Centered()
            );

            // Get worker details first to verify it exists
            var existingWorker = await workerService.GetWorkerById(workerId);
            if (existingWorker.Data is null)
            {
                throw new InvalidOperationException(existingWorker.Message);
            }

            var response = await workerService.DeleteWorker(existingWorker.Data.WorkerId);
            
            if (response.RequestFailed)
            {
                throw new InvalidOperationException(response.Message);
            }
            
            // Log success but don't display UI here - that's handled in the menu
            Console.WriteLine($"Worker deleted successfully: {response.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete worker: {ex.Message}", ex);
        }
    }

    public async Task<WorkerFilterOptions> GetWorkerFilterInputAsync()
    {
        return await Task.FromResult(userInterface.FilterWorkersUi());
    }

    public async Task GetAllWorkersWithData(WorkerFilterOptions filterOptions)
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]View All Workers - Results[/]").RuleStyle("yellow").Centered()
            );

            var response = await workerService.GetAllWorkers(filterOptions);

            if (response.Data is null)
            {
                throw new InvalidOperationException("No workers found.");
            }
            else
            {
                userInterface.DisplayWorkersTable(response.Data);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get all workers: {ex.Message}", ex);
        }
    }
}
