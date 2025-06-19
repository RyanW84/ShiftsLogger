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
    public async Task<ApiResponseDto<Workers>> CheckWorkerExists(int workerId)
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
                    return new ApiResponseDto<Workers>
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
            return new ApiResponseDto<Workers>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Exception occurred: {ex.Message}",
                Data = null,
            };
        }
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
            var workers = await workerService.GetAllWorkers(workerFilterOptions);

            if (workers.Data is not null)
            {
                userInterface.DisplayWorkersTable(workers.Data);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]No workers found.[/]");
                userInterface.ContinueAndClearScreen();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
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
            var workerId = userInterface.GetWorkerByIdUi();
            var worker = await CheckWorkerExists(workerId);

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

            var workerId = userInterface.GetWorkerByIdUi();

            var existingWorker = await CheckWorkerExists(workerId);

            if (existingWorker.Data is null)
            {
                userInterface.DisplayErrorMessage(existingWorker.Message);
                userInterface.ContinueAndClearScreen();
                return;
            }

            var updatedWorker = userInterface.UpdateWorkerUi(existingWorker.Data);

            var updatedWorkerResponse = await workerService.UpdateWorker(workerId, updatedWorker);
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

            var workerId = userInterface.GetWorkerByIdUi();
            var existingWorker = await CheckWorkerExists(workerId);

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
}
