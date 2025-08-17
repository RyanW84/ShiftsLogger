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

    public WorkerService(HttpClient httpClient, IConfiguration configuration, ILogger<WorkerService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        // Set base address if not already set
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("ApiBaseUrl") ?? "http://localhost:5181/");
        }
    }

    // ...existing methods...

    public async Task<ApiResponseDto<List<Worker>>> GetWorkersByFilterAsync(ConsoleFrontEnd.Models.FilterOptions.WorkerFilterOptions filter)
    {
        try
        {
            var allResponse = await GetAllWorkersAsync();
            if (allResponse.RequestFailed || allResponse.Data == null)
                return allResponse;

            var filtered = allResponse.Data.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filter.Name))
                filtered = filtered.Where(w => w.Name != null && w.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(filter.Email))
                filtered = filtered.Where(w => w.Email != null && w.Email.Contains(filter.Email, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(filter.PhoneNumber))
                filtered = filtered.Where(w => w.PhoneNumber != null && w.PhoneNumber.Contains(filter.PhoneNumber));
            if (filter.WorkerId.HasValue)
                filtered = filtered.Where(w => w.WorkerId == filter.WorkerId.Value);

            var resultList = filtered.ToList();
            return new ApiResponseDto<List<Worker>>("Filtered workers")
            {
                Data = resultList,
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                TotalCount = resultList.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering workers");
            return new ApiResponseDto<List<Worker>>("Error filtering workers")
            {
                Data = new List<Worker>(),
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError
            };
        }
    }

    public async Task<ApiResponseDto<List<Worker>>> GetAllWorkersAsync()
    {
    // ...existing code...

        try
        {
            var queryString = "api/workers";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve workers. Status: {StatusCode}", response.StatusCode);
                return new ApiResponseDto<List<Worker>>("Failed to retrieve workers")
                {
                    ResponseCode = response.StatusCode,
                    Message = "Failed to retrieve workers",
                    Data = new List<Worker>(),
                    RequestFailed = true
                };
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
            return new ApiResponseDto<List<Worker>>("Error occurred while fetching workers from API")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = ex.Message,
                Data = new List<Worker>(),
                RequestFailed = true
            };
        }
    }

    // ...existing code...

    public async Task<ApiResponseDto<Worker?>> GetWorkerByIdAsync(int id)
    {
    // ...existing code...

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
            return new ApiResponseDto<Worker?>("Error occurred while fetching worker from API")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = ex.Message,
                Data = null,
                RequestFailed = true
            };
        }
    }

    // ...existing code...

    public async Task<ApiResponseDto<Worker>> CreateWorkerAsync(Worker worker)
    {
    // ...existing code...

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/workers", worker);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                _logger.LogError("Error creating worker. Status Code: {StatusCode}", response.StatusCode);
                return new ApiResponseDto<Worker>("Error creating worker")
                {
                    ResponseCode = response.StatusCode,
                    RequestFailed = true,
                    Data = null
                };
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
            return new ApiResponseDto<Worker>("Error occurred while creating worker")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    // ...existing code...

    public async Task<ApiResponseDto<Worker?>> UpdateWorkerAsync(int id, Worker updatedWorker)
    {
    // ...existing code...

        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/workers/{id}", updatedWorker);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Error updating worker {WorkerId}. Status Code: {StatusCode}", id, response.StatusCode);
                return new ApiResponseDto<Worker?>("Error updating worker")
                {
                    ResponseCode = response.StatusCode,
                    RequestFailed = true,
                    Data = null
                };
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
            return new ApiResponseDto<Worker?>("Error occurred while updating worker")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    // ...existing code...

    public async Task<ApiResponseDto<string?>> DeleteWorkerAsync(int id)
    {
    // ...existing code...

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
