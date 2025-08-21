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
            _httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("ApiBaseUrl") ?? "https://localhost:7009");
        }
    }

    public async Task<ApiResponseDto<List<Worker>>> GetWorkersByFilterAsync(ConsoleFrontEnd.Models.FilterOptions.WorkerFilterOptions filter)
    {
        try
        {
            var queryString = $"api/workers?" + BuildWorkerFilterQuery(filter);
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<Worker>>(
                response,
                _logger,
                "Get Workers By Filter",
                new List<Worker>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering workers via API");
            return new ApiResponseDto<List<Worker>>($"Filter Error: {ex.Message}")
            {
                Data = new List<Worker>(),
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError
            };
        }
    }

    private string BuildWorkerFilterQuery(ConsoleFrontEnd.Models.FilterOptions.WorkerFilterOptions filter)
    {
        var query = new List<string>();
        if (filter.WorkerId.HasValue) query.Add($"WorkerId={filter.WorkerId.Value}");
        if (!string.IsNullOrWhiteSpace(filter.Name)) query.Add($"Name={Uri.EscapeDataString(filter.Name)}");
        if (!string.IsNullOrWhiteSpace(filter.Email)) query.Add($"Email={Uri.EscapeDataString(filter.Email)}");
        if (!string.IsNullOrWhiteSpace(filter.PhoneNumber)) query.Add($"PhoneNumber={Uri.EscapeDataString(filter.PhoneNumber)}");
        if (!string.IsNullOrWhiteSpace(filter.Search)) query.Add($"Search={Uri.EscapeDataString(filter.Search)}");
        return string.Join("&", query);
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
        var dto = new ConsoleFrontEnd.Models.Dtos.WorkerApiRequestDto
        {
            Name = worker.Name,
            Email = worker.Email,
            PhoneNumber = worker.PhoneNumber
        };
        var errors = Services.Validation.WorkerValidation.Validate(dto);
        if (errors.Count > 0)
        {
            return new ApiResponseDto<Worker>("Validation failed")
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                Data = null,
                Message = string.Join("; ", errors)
            };
        }
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/workers", dto);
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
        var dto = new ConsoleFrontEnd.Models.Dtos.WorkerApiRequestDto
        {
            Name = updatedWorker.Name,
            Email = updatedWorker.Email,
            PhoneNumber = updatedWorker.PhoneNumber
        };
        var errors = Services.Validation.WorkerValidation.Validate(dto);
        if (errors.Count > 0)
        {
            return new ApiResponseDto<Worker?>("Validation failed")
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                Data = null,
                Message = string.Join("; ", errors)
            };
        }
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/workers/{id}", dto);
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
