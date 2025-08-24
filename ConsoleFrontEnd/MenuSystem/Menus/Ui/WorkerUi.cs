using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.MenuSystem;

/// <summary>
/// Refactored Worker UI implementation following SOLID principles with reduced code duplication
/// </summary>
public class WorkerUi : IWorkerUi
{
    private readonly UiHelper _uiHelper;
    private const string EntityName = "Worker";
    private const string EntityPluralName = "Workers";

    public WorkerUi(IConsoleDisplayService display, ILogger<WorkerUi> logger)
    {
        _uiHelper = new UiHelper(display, logger);
    }

    public Worker CreateWorkerUi()
    {
        _uiHelper.DisplayCreateHeader(EntityName);
        
        var name = _uiHelper.GetRequiredStringInput("Enter name");
        var email = _uiHelper.GetOptionalStringInput("Enter email");
        var phone = _uiHelper.GetOptionalStringInput("Enter phone number");

        // Validate email if provided
        if (!string.IsNullOrEmpty(email) && !_uiHelper.IsValidEmail(email))
        {
            _uiHelper.DisplayValidationError("Invalid email format.");
            return CreateWorkerUi(); // Retry
        }

        return new Worker
        {
            WorkerId = 0, // Will be assigned by service
            Name = name,
            Email = email,
            PhoneNumber = phone
        };
    }

    public Worker UpdateWorkerUi(Worker existingWorker)
    {
        _uiHelper.DisplayUpdateHeader(EntityName, existingWorker.Name);
        
        var name = _uiHelper.GetOptionalStringInput("Enter name", existingWorker.Name) ?? existingWorker.Name;
        var email = _uiHelper.GetOptionalStringInput("Enter email", existingWorker.Email);
        var phone = _uiHelper.GetOptionalStringInput("Enter phone number", existingWorker.PhoneNumber);

        // Validate email if provided
        if (!string.IsNullOrEmpty(email) && !_uiHelper.IsValidEmail(email))
        {
            _uiHelper.DisplayValidationError("Invalid email format.");
            return UpdateWorkerUi(existingWorker); // Retry
        }

        return new Worker
        {
            WorkerId = existingWorker.Id,
            Name = name,
            Email = email,
            PhoneNumber = phone
        };
    }

    public WorkerFilterOptions FilterWorkersUi()
    {
        _uiHelper.DisplayFilterHeader(EntityPluralName);
        
        var name = _uiHelper.GetOptionalStringInput("Filter by name");
        var email = _uiHelper.GetOptionalStringInput("Filter by email");
        var phone = _uiHelper.GetOptionalStringInput("Filter by phone");

        return new WorkerFilterOptions
        {
            Name = name,
            Email = email,
            PhoneNumber = phone
        };
    }

    public void DisplayWorkersTable(IEnumerable<Worker> workers)
    {
        _uiHelper.DisplayEntitiesTable(workers, EntityPluralName);
    }

    public int GetWorkerByIdUi()
    {
        return _uiHelper.GetEntityIdInput(EntityName);
    }

    public int SelectWorker()
    {
        return _uiHelper.SelectEntityInput(EntityName);
    }
}
