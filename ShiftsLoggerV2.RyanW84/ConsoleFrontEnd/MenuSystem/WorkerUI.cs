using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.MenuSystem;

public class WorkerUI : IWorkerUi
{
    private readonly UserInterface _userInterface;

    public WorkerUI(UserInterface userInterface)
    {
        _userInterface = userInterface;
    }

    public Worker CreateWorker() => _userInterface.CreateWorkerUi();
    public Worker UpdateWorker(Worker existingWorker) => _userInterface.UpdateWorkerUi(existingWorker);
    public WorkerFilterOptions FilterWorkers() => _userInterface.FilterWorkersUi();
    public void DisplayWorkersTable(IEnumerable<Worker> workers) => _userInterface.DisplayWorkersTable(workers);
    public int GetWorkerById() => _userInterface.GetWorkerByIdUi();
}
namespace ConsoleFrontEnd.MenuSystem;

public class WorkerUI : IWorkerUi
{
    private readonly UserInterface _userInterface;

    public WorkerUI(UserInterface userInterface)
    {
        _userInterface = userInterface;
    }

    public Worker CreateWorkerUi() => _userInterface.CreateWorkerUi();
    public Worker UpdateWorkerUi(Worker existingWorker) => _userInterface.UpdateWorkerUi(existingWorker);
    public WorkerFilterOptions FilterWorkersUi() => _userInterface.FilterWorkersUi();
    public void DisplayWorkersTable(IEnumerable<Worker> workers) => _userInterface.DisplayWorkersTable(workers);
    public int GetWorkerByIdUi() => _userInterface.GetWorkerByIdUi();
}
