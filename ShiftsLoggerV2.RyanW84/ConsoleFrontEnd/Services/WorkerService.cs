using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services;

public class WorkerService : IWorkerService
{
    public Task<ApiResponseDto<List<Worker>>> GetAllWorkersAsync()
    {
        // Mock implementation using correct property names
        var workers = new List<Worker>
        {
            new Worker 
            { 
                WorkerId = 1, 
                Name = "John Doe",
                PhoneNumber = "+44 123 456 7890",
                Email = "john.doe@company.com"
            },
            new Worker 
            { 
                WorkerId = 2, 
                Name = "Jane Smith",
                PhoneNumber = "+44 987 654 3210",
                Email = "jane.smith@company.com"
            }
        };

        return Task.FromResult(new ApiResponseDto<List<Worker>>("Success")
        {
            Data = workers,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        });
    }

    public Task<ApiResponseDto<Worker?>> GetWorkerByIdAsync(int id)
    {
        // Mock implementation using correct property names
        var worker = new Worker 
        { 
            WorkerId = id, 
            Name = $"Worker {id}",
            PhoneNumber = $"+44 123 456 78{id:00}",
            Email = $"worker{id}@company.com"
        };

        return Task.FromResult(new ApiResponseDto<Worker?>("Success")
        {
            Data = worker,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        });
    }

    public Task<ApiResponseDto<Worker>> CreateWorkerAsync(Worker worker)
    {
        // Mock implementation - assign an ID
        worker.WorkerId = new Random().Next(1, 1000);

        return Task.FromResult(new ApiResponseDto<Worker>("Worker created successfully")
        {
            Data = worker,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.Created
        });
    }

    public Task<ApiResponseDto<Worker?>> UpdateWorkerAsync(int id, Worker updatedWorker)
    {
        // Mock implementation
        updatedWorker.WorkerId = id;

        return Task.FromResult(new ApiResponseDto<Worker?>("Worker updated successfully")
        {
            Data = updatedWorker,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        });
    }

    public Task<ApiResponseDto<string?>> DeleteWorkerAsync(int id)
    {
        // Mock implementation
        return Task.FromResult(new ApiResponseDto<string?>("Worker deleted successfully")
        {
            Data = $"Deleted worker with ID {id}",
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        });
    }
}
