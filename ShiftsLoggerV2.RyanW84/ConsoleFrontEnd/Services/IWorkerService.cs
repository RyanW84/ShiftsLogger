using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.Services;
public interface IWorkerService
{
	public Task<ApiResponseDto<List<Worker>>> GetAllWorkers(WorkerFilterOptions workerOptions);
	public Task<ApiResponseDto<Worker>> GetWorkerById(int id);
	public Task<ApiResponseDto<Worker>> CreateWorker(Worker createdWorker);
	public Task<ApiResponseDto<Worker>> UpdateWorker(int id , Worker updatedWorker);
	public Task<ApiResponseDto<string>> DeleteWorker(int id);
}
