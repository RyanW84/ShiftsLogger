using ConsoleFrontEnd.Interfaces;
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
    public WorkerFilterOptions GetWorkerFilterOptions() => _userInterface.FilterWorkersUi();
    public void DisplayWorkers(IEnumerable<Worker> workers) => _userInterface.DisplayWorkersTable(workers);
    public int SelectWorker() => _userInterface.GetWorkerByIdUi();
}
