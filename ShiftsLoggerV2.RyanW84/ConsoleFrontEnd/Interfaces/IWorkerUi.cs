using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.Interfaces;

public interface IWorkerUi
{
    Worker CreateWorkerUi();
    Worker UpdateWorkerUi(Worker existingWorker);
    WorkerFilterOptions FilterWorkersUi();
    void DisplayWorkersTable(IEnumerable<Worker> workers);
    int GetWorkerByIdUi();
}