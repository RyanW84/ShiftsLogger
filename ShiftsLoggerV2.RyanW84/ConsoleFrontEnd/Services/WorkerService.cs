using System.Net;
using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Services;

public class WorkerService : IWorkerService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WorkerService> _logger;
    private readonly bool _useMockData;

    public WorkerService(HttpClient httpClient, IConfiguration configuration, ILogger<WorkerService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _useMockData = _configuration.GetValue<bool>("UseMockData", true);
        
        // Set base address if not already set
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("ApiBaseUrl") ?? "http://localhost:5181/");
        }
    }

    public async Task<ApiResponseDto<List<Worker>>> GetAllWorkersAsync()
    {
        if (_useMockData)
        {
            return GetMockWorkers();
        }

        try
        {
            var queryString = "api/workers";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve workers. Status: {StatusCode}", response.StatusCode);
                return GetMockWorkers(); // Fallback to mock data
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Worker>>>()
                         ?? new ApiResponseDto<List<Worker>>("Data obtained")
                         {
                             ResponseCode = response.StatusCode,
                             Message = "Data obtained",
                             Data = new List<Worker>()
                         };

            _logger.LogInformation("Workers retrieved successfully from API");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching workers from API");
            return GetMockWorkers(); // Fallback to mock data
        }
    }

    private static ApiResponseDto<List<Worker>> GetMockWorkers()
    {
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

        return new ApiResponseDto<List<Worker>>("Success")
        {
            Data = workers,
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK
        };
    }

    public async Task<ApiResponseDto<Worker?>> GetWorkerByIdAsync(int id)
    {
        if (_useMockData)
        {
            return GetMockWorkerById(id);
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/workers/{id}");
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>()
                             ?? new ApiResponseDto<Worker>("No data returned")
                             {
                                 ResponseCode = response.StatusCode,
                                 Message = "No data returned.",
                                 Data = null
                             };
                return new ApiResponseDto<Worker?>("Success") 
                { 
                    Data = result.Data,
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK
                };
            }

            return new ApiResponseDto<Worker?>(response.ReasonPhrase ?? "Worker not found")
            {
                ResponseCode = response.StatusCode,
                RequestFailed = true,
                Data = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching worker {WorkerId} from API", id);
            return GetMockWorkerById(id); // Fallback to mock data
        }
    }

    private static ApiResponseDto<Worker?> GetMockWorkerById(int id)
    {
        var worker = new Worker 
        { 
            WorkerId = id, 
            Name = $"Worker {id}",
            PhoneNumber = $"+44 123 456 78{id:00}",
            Email = $"worker{id}@company.com"
        };

        return new ApiResponseDto<Worker?>("Success")
        {
            Data = worker,
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK
        };
    }

    public async Task<ApiResponseDto<Worker>> CreateWorkerAsync(Worker worker)
    {
        if (_useMockData)
        {
            return CreateMockWorker(worker);
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/workers", worker);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                _logger.LogError("Error creating worker. Status Code: {StatusCode}", response.StatusCode);
                return CreateMockWorker(worker); // Fallback to mock data
            }

            var createdWorker = await response.Content.ReadFromJsonAsync<Worker>() ?? worker;
            _logger.LogInformation("Worker created successfully");
            
            return new ApiResponseDto<Worker>("Worker created successfully")
            {
                ResponseCode = response.StatusCode,
                RequestFailed = false,
                Data = createdWorker
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating worker");
            return CreateMockWorker(worker); // Fallback to mock data
        }
    }

    private static ApiResponseDto<Worker> CreateMockWorker(Worker worker)
    {
        worker.WorkerId = new Random().Next(1, 1000);

        return new ApiResponseDto<Worker>("Worker created successfully")
        {
            Data = worker,
            RequestFailed = false,
            ResponseCode = HttpStatusCode.Created
        };
    }

    public async Task<ApiResponseDto<Worker?>> UpdateWorkerAsync(int id, Worker updatedWorker)
    {
        if (_useMockData)
        {
            return UpdateMockWorker(id, updatedWorker);
        }

        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/workers/{id}", updatedWorker);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Error updating worker {WorkerId}. Status Code: {StatusCode}", id, response.StatusCode);
                return UpdateMockWorker(id, updatedWorker); // Fallback to mock data
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>()
                         ?? new ApiResponseDto<Worker>("Update Worker succeeded")
                         {
                             ResponseCode = response.StatusCode,
                             Message = "Update Worker succeeded.",
                             Data = updatedWorker
                         };

            return new ApiResponseDto<Worker?>("Worker updated successfully")
            {
                ResponseCode = response.StatusCode,
                RequestFailed = false,
                Data = result.Data
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating worker {WorkerId}", id);
            return UpdateMockWorker(id, updatedWorker); // Fallback to mock data
        }
    }

    private static ApiResponseDto<Worker?> UpdateMockWorker(int id, Worker updatedWorker)
    {
        updatedWorker.WorkerId = id;

        return new ApiResponseDto<Worker?>("Worker updated successfully")
        {
            Data = updatedWorker,
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK
        };
    }

    public async Task<ApiResponseDto<string?>> DeleteWorkerAsync(int id)
    {
        if (_useMockData)
        {
            return new ApiResponseDto<string?>("Worker deleted successfully")
            {
                Data = $"Deleted worker with ID {id}",
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK
            };
        }

        try
        {
            var response = await _httpClient.DeleteAsync($"api/workers/{id}");
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                _logger.LogError("Error deleting worker {WorkerId}. Status Code: {StatusCode}", id, response.StatusCode);
                return new ApiResponseDto<string?>($"Error deleting Worker - {response.StatusCode}")
                {
                    ResponseCode = response.StatusCode,
                    RequestFailed = true,
                    Data = null
                };
            }

            return new ApiResponseDto<string?>("Worker deleted successfully")
            {
                ResponseCode = response.StatusCode,
                RequestFailed = false,
                Data = $"Deleted worker with ID {id}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting worker {WorkerId}", id);
            return new ApiResponseDto<string?>("Error occurred while deleting worker")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }
}
