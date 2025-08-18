using System.Net;
using System.Net.Http.Json;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Services;

public class ShiftService : IShiftService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ShiftService> logger)
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

    public async Task<ApiResponseDto<List<Shift>>> GetShiftsByFilterAsync(ConsoleFrontEnd.Models.FilterOptions.ShiftFilterOptions filter)
    {
        try
        {
            var allResponse = await GetAllShiftsAsync();
            if (allResponse.RequestFailed || allResponse.Data == null)
                return allResponse;

            return FilterShiftsLocally(allResponse, filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering shifts");
            return new ApiResponseDto<List<Shift>>($"Filter Error: {ex.Message}")
            {
                Data = new List<Shift>(),
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError
            };
        }
    }

    private static ApiResponseDto<List<Shift>> FilterShiftsLocally(ApiResponseDto<List<Shift>> allResponse, ConsoleFrontEnd.Models.FilterOptions.ShiftFilterOptions filter)
    {
        var filtered = (allResponse.Data ?? new List<Shift>()).AsQueryable();
        
        if (filter.ShiftId.HasValue)
            filtered = filtered.Where(s => s.ShiftId == filter.ShiftId.Value);
        if (filter.WorkerId.HasValue)
            filtered = filtered.Where(s => s.WorkerId == filter.WorkerId.Value);
        if (filter.LocationId.HasValue)
            filtered = filtered.Where(s => s.LocationId == filter.LocationId.Value);
        if (filter.StartTime.HasValue)
            filtered = filtered.Where(s => s.StartTime >= filter.StartTime.Value);
        if (filter.EndTime.HasValue)
            filtered = filtered.Where(s => s.EndTime <= filter.EndTime.Value);
        if (!string.IsNullOrWhiteSpace(filter.LocationName))
            filtered = filtered.Where(s => s.Location != null && s.Location.Name.Contains(filter.LocationName, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(filter.WorkerName))
            filtered = filtered.Where(s => s.Worker != null && s.Worker.Name.Contains(filter.WorkerName, StringComparison.OrdinalIgnoreCase));

        var resultList = filtered.ToList();
        return new ApiResponseDto<List<Shift>>("Filtered shifts successfully")
        {
            Data = resultList,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            TotalCount = resultList.Count
        };
    }

    public async Task<ApiResponseDto<List<Shift>>> GetAllShiftsAsync()
    {
        try
        {
            var queryString = "api/shifts";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<Shift>>(
                response,
                _logger,
                "Get All Shifts",
                new List<Shift>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching shifts from API");
            return new ApiResponseDto<List<Shift>>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                Data = new List<Shift>(),
                RequestFailed = true
            };
        }
    }

    public async Task<ApiResponseDto<Shift?>> GetShiftByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/shifts/{id}");
            return await HttpResponseHelper.HandleHttpResponseAsync<Shift?>(
                response,
                _logger,
                $"Get Shift {id}",
                null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching shift {ShiftId}", id);
            return new ApiResponseDto<Shift?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Shift>> CreateShiftAsync(Shift shift)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/shifts", shift);
            return await HttpResponseHelper.HandleHttpResponseAsync<Shift>(
                response,
                _logger,
                "Create Shift",
                shift
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating shift");
            return new ApiResponseDto<Shift>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Shift?>> UpdateShiftAsync(int id, Shift updatedShift)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/shifts/{id}", updatedShift);
            return await HttpResponseHelper.HandleHttpResponseAsync<Shift?>(
                response,
                _logger,
                $"Update Shift {id}",
                updatedShift
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating shift {ShiftId}", id);
            return new ApiResponseDto<Shift?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<string?>> DeleteShiftAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/shifts/{id}");
            return await HttpResponseHelper.HandleHttpResponseAsync<string?>(
                response,
                _logger,
                $"Delete Shift {id}",
                $"Deleted shift with ID {id}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting shift {ShiftId}", id);
            return new ApiResponseDto<string?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }
}
