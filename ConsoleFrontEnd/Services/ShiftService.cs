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
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(HttpClient httpClient, ILogger<ShiftService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponseDto<List<Shift>>> GetShiftsByFilterAsync(ConsoleFrontEnd.Models.FilterOptions.ShiftFilterOptions filter)
    {
        try
        {
            var queryString = $"api/shifts?" + BuildShiftFilterQuery(filter);
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<Shift>>(
                response,
                _logger,
                "Get Shifts By Filter",
                new List<Shift>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering shifts via API");
            return new ApiResponseDto<List<Shift>>($"Filter Error: {ex.Message}")
            {
                Data = new List<Shift>(),
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError
            };
        }
    }

    private string BuildShiftFilterQuery(ConsoleFrontEnd.Models.FilterOptions.ShiftFilterOptions filter)
    {
        var query = new List<string>();
        if (filter.ShiftId.HasValue) query.Add($"ShiftId={filter.ShiftId.Value}");
        if (filter.WorkerId.HasValue) query.Add($"WorkerId={filter.WorkerId.Value}");
        if (filter.LocationId.HasValue) query.Add($"LocationId={filter.LocationId.Value}");
        if (filter.StartTime.HasValue) query.Add($"StartTime={Uri.EscapeDataString(filter.StartTime.Value.ToString("o"))}");
        if (filter.EndTime.HasValue) query.Add($"EndTime={Uri.EscapeDataString(filter.EndTime.Value.ToString("o"))}");
        if (!string.IsNullOrWhiteSpace(filter.LocationName)) query.Add($"LocationName={Uri.EscapeDataString(filter.LocationName)}");
        if (!string.IsNullOrWhiteSpace(filter.WorkerName)) query.Add($"WorkerName={Uri.EscapeDataString(filter.WorkerName)}");
        return string.Join("&", query);
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
        var dto = new ConsoleFrontEnd.Models.Dtos.ShiftApiRequestDto
        {
            WorkerId = shift.WorkerId,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            LocationId = shift.LocationId
        };
        var errors = Services.Validation.ShiftValidation.Validate(dto);
        if (errors.Count > 0)
        {
            return new ApiResponseDto<Shift>("Validation failed")
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                Data = null,
                Message = string.Join("; ", errors)
            };
        }
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/shifts", dto);
            var handled = await HttpResponseHelper.HandleHttpResponseAsync<Shift>(
                response,
                _logger,
                "Create Shift",
                shift
            );

            // If server returned the created Shift but omitted navigation properties, fetch it by ID
            if (!handled.RequestFailed && handled.Data != null && (handled.Data.Worker == null || handled.Data.Location == null))
            {
                var refreshed = await GetShiftByIdAsync(handled.Data.ShiftId);
                if (!refreshed.RequestFailed && refreshed.Data != null)
                {
                    return new ApiResponseDto<Shift>(refreshed.Message ?? "Shift retrieved")
                    {
                        Data = refreshed.Data,
                        RequestFailed = false,
                        ResponseCode = refreshed.ResponseCode,
                        TotalCount = refreshed.TotalCount
                    };
                }
            }

            return handled;
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
        var dto = new ConsoleFrontEnd.Models.Dtos.ShiftApiRequestDto
        {
            WorkerId = updatedShift.WorkerId,
            StartTime = updatedShift.StartTime,
            EndTime = updatedShift.EndTime,
            LocationId = updatedShift.LocationId
        };
        var errors = Services.Validation.ShiftValidation.Validate(dto);
        if (errors.Count > 0)
        {
            return new ApiResponseDto<Shift?>("Validation failed")
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                Data = null,
                Message = string.Join("; ", errors)
            };
        }
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/shifts/{id}", dto);
            var handled = await HttpResponseHelper.HandleHttpResponseAsync<Shift?>(
                response,
                _logger,
                $"Update Shift {id}",
                updatedShift
            );

            // If server returned only IDs (no navigation properties), fetch the fully populated shift
            if (!handled.RequestFailed && handled.Data != null && (handled.Data.Worker == null || handled.Data.Location == null))
            {
                var refreshed = await GetShiftByIdAsync(id);
                if (!refreshed.RequestFailed && refreshed.Data != null)
                    return refreshed;
            }

            return handled;
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
