using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.MenuSystem;

public interface IWorkerUi
{
    Worker CreateWorker();
    Worker UpdateWorker(Worker existingWorker);
    WorkerFilterOptions FilterWorkers();
    void DisplayWorkersTable(IEnumerable<Worker> workers);
    int GetWorkerById();
}
