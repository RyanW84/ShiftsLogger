// Core interfaces

using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.MenuSystem;

public interface IDisplayService
{
    void DisplaySuccessMessage(string message);
    void DisplayErrorMessage(string message);
    void ContinueAndClearScreen();
}

public interface IInputService
{
    T GetInput<T>(string prompt, T? defaultValue = default);
    T GetValidatedInput<T>(string prompt, Func<T, bool> validator, string errorMessage, T? defaultValue = default);
    string GetSelection(string title, IEnumerable<string> choices);
    DateTime GetDateTime(string prompt, DateTime? minValue = null);
}

// Entity-specific UI interfaces
public interface IShiftUi
{
    Shift CreateShift(int workerId);
    Shift UpdateShift(Shift existingShift);
    ShiftFilterOptions GetShiftFilterOptions();
    void DisplayShifts(IEnumerable<Shift> shifts);
    int SelectShift();
}

public interface IWorkerUi
{
    Worker CreateWorker();
    Worker UpdateWorker(Worker existingWorker);
    WorkerFilterOptions GetWorkerFilterOptions();
    void DisplayWorkers(IEnumerable<Worker> workers);
    int SelectWorker();
}

public interface ILocationUi
{
    Location CreateLocation();
    Location UpdateLocation(Location existingLocation);
    LocationFilterOptions GetLocationFilterOptions();
    void DisplayLocations(IEnumerable<Location> locations);
    int SelectLocation();
}