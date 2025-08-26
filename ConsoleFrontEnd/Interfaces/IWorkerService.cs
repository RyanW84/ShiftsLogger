using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services;

public interface IWorkerService
{
    Task<ApiResponseDto<List<Worker>>> GetAllWorkersAsync();
    Task<ApiResponseDto<Worker?>> GetWorkerByIdAsync(int id);
    Task<ApiResponseDto<Worker?>> GetWorkerByNameAsync(string name);
    Task<ApiResponseDto<Worker>> CreateWorkerAsync(Worker worker);
    Task<ApiResponseDto<Worker?>> UpdateWorkerAsync(int id, Worker updatedWorker);
    Task<ApiResponseDto<bool>> DeleteWorkerAsync(int id);
    Task<ApiResponseDto<List<Worker>>> GetWorkersByFilterAsync(ConsoleFrontEnd.Models.FilterOptions.WorkerFilterOptions filter);
}
