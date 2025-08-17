using System.Net;
using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Services;

public class WorkerService : IWorkerService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WorkerService> _logger;

    public WorkerService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<WorkerService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ShiftsLoggerApi");
        _configuration = configuration;
        _logger = logger;
        
        // Set base address if not already set
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("ApiBaseUrl") ?? "http://localhost:5181/");
        }
    }

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
            if (!string.IsNullOrWhiteSpace(filter.Search))
                filtered = filtered.Where(w => (w.Name != null && w.Name.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)) ||
                                               (w.Email != null && w.Email.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)) ||
                                               (w.PhoneNumber != null && w.PhoneNumber.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)));

            var resultList = filtered.ToList();
            return new ApiResponseDto<List<Worker>>("Filtered workers successfully")
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
            return new ApiResponseDto<List<Worker>>($"Filter Error: {ex.Message}")
            {
                Data = new List<Worker>(),
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError
            };
        }
    }

    public async Task<ApiResponseDto<List<Worker>>> GetAllWorkersAsync()
    {
        try
        {
            var queryString = "api/workers";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<Worker>>(
                response,
                _logger,
                "Get All Workers",
                new List<Worker>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching workers from API");
            return new ApiResponseDto<List<Worker>>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                Data = new List<Worker>(),
                RequestFailed = true
            };
        }
    }

    public async Task<ApiResponseDto<Worker?>> GetWorkerByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/workers/{id}");
            return await HttpResponseHelper.HandleHttpResponseAsync<Worker?>(
                response,
                _logger,
                $"Get Worker {id}",
                null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching worker {WorkerId}", id);
            return new ApiResponseDto<Worker?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Worker>> CreateWorkerAsync(Worker worker)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/workers", worker);
            return await HttpResponseHelper.HandleHttpResponseAsync<Worker>(
                response,
                _logger,
                "Create Worker",
                worker
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating worker");
            return new ApiResponseDto<Worker>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Worker?>> UpdateWorkerAsync(int id, Worker updatedWorker)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/workers/{id}", updatedWorker);
            return await HttpResponseHelper.HandleHttpResponseAsync<Worker?>(
                response,
                _logger,
                $"Update Worker {id}",
                updatedWorker
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating worker {WorkerId}", id);
            return new ApiResponseDto<Worker?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<string?>> DeleteWorkerAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/workers/{id}");
            return await HttpResponseHelper.HandleHttpResponseAsync<string?>(
                response,
                _logger,
                $"Delete Worker {id}",
                $"Deleted worker with ID {id}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting worker {WorkerId}", id);
            return new ApiResponseDto<string?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }
}
