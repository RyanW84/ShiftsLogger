using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public class WorkerUI : IWorkerUi
{
    private readonly IConsoleDisplayService _display;

    public WorkerUI(IConsoleDisplayService display)
    {
        _display = display;
    }

    public Worker CreateWorkerUi()
    {
        _display.DisplayHeader("Create New Worker");
        
        var firstName = AnsiConsole.Ask<string>("[green]Enter first name:[/]");
        var lastName = AnsiConsole.Ask<string>("[green]Enter last name:[/]");
        var email = AnsiConsole.Ask<string>("[green]Enter email:[/]");
        var phone = AnsiConsole.Ask<string>("[green]Enter phone number:[/]");
        
        return new Worker
        {
            WorkerId = 0, // Will be assigned by service
            Name = $"{firstName} {lastName}".Trim(),
            Email = email,
            PhoneNumber = phone
        };
    }

    public Worker UpdateWorkerUi(Worker existingWorker)
    {
        _display.DisplayHeader($"Update Worker: {existingWorker.Name}");
        
        var firstName = AnsiConsole.Ask<string>("[green]Enter first name:[/]", existingWorker.FirstName);
        var lastName = AnsiConsole.Ask<string>("[green]Enter last name:[/]", existingWorker.LastName);
        var email = AnsiConsole.Ask<string>("[green]Enter email:[/]", existingWorker.Email ?? string.Empty);
        var phone = AnsiConsole.Ask<string>("[green]Enter phone number:[/]", existingWorker.PhoneNumber ?? string.Empty);
        
        return new Worker
        {
            WorkerId = existingWorker.Id,
            Name = $"{firstName} {lastName}".Trim(),
            Email = email,
            PhoneNumber = phone
        };
    }

    public WorkerFilterOptions FilterWorkersUi()
    {
        _display.DisplayHeader("Filter Workers");
        
        var firstName = AnsiConsole.Ask<string>("[yellow]Filter by first name (press Enter to skip):[/]", string.Empty);
        var lastName = AnsiConsole.Ask<string>("[yellow]Filter by last name (press Enter to skip):[/]", string.Empty);
        var email = AnsiConsole.Ask<string>("[yellow]Filter by email (press Enter to skip):[/]", string.Empty);
        
        return new WorkerFilterOptions
        {
            FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName,
            LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName,
            Email = string.IsNullOrWhiteSpace(email) ? null : email
        };
    }

    public void DisplayWorkersTable(IEnumerable<Worker> workers)
    {
        _display.DisplayTable(workers, "Workers");
    }

    public int GetWorkerByIdUi()
    {
        return AnsiConsole.Ask<int>("[green]Enter worker ID:[/]");
    }

    public int SelectWorker()
    {
        return AnsiConsole.Ask<int>("[green]Select worker ID:[/]");
    }
}
