using System.Net;
using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Services;

public class ShiftService : IShiftService
{
    public async Task<ApiResponseDto<List<Shift>>> GetShiftsByFilterAsync(ConsoleFrontEnd.Models.FilterOptions.ShiftFilterOptions filter)
    {
        if (_useMockData)
        {
            var allShifts = GetMockShifts();
            return FilterShiftsLocally(allShifts, filter);
        }
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
            return new ApiResponseDto<List<Shift>>("Error filtering shifts")
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
            filtered = filtered.Where(s => s.LocationId.ToString() == filter.LocationName);
        // Add more filters as needed
        var resultList = filtered.ToList();
        return new ApiResponseDto<List<Shift>>("Filtered shifts")
        {
            Data = resultList,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            TotalCount = resultList.Count
        };
    }

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ShiftService> _logger;
    private readonly bool _useMockData;

    public ShiftService(HttpClient httpClient, IConfiguration configuration, ILogger<ShiftService> logger)
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

    public async Task<ApiResponseDto<List<Shift>>> GetAllShiftsAsync()
    {
        if (_useMockData)
        {
            return GetMockShifts();
        }

        try
        {
            var queryString = "api/shifts";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString);
            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => "No shifts found (404).",
                    System.Net.HttpStatusCode.BadRequest => "Bad request (400) while retrieving shifts.",
                    System.Net.HttpStatusCode.InternalServerError => "Server error (500) while retrieving shifts.",
                    System.Net.HttpStatusCode.Unauthorized => "Unauthorized (401) while retrieving shifts.",
                    System.Net.HttpStatusCode.Forbidden => "Forbidden (403) while retrieving shifts.",
                    System.Net.HttpStatusCode.Conflict => "Conflict (409) while retrieving shifts.",
                    System.Net.HttpStatusCode.RequestTimeout => "Request Timeout (408) while retrieving shifts.",
                    (System.Net.HttpStatusCode)422 => "Unprocessable Entity (422) while retrieving shifts.",
                    _ => $"Failed to retrieve shifts: {response.ReasonPhrase}"
                };
                _logger.LogError(errorMsg);
                return new ApiResponseDto<List<Shift>>(errorMsg)
                {
                    RequestFailed = true,
                    ResponseCode = response.StatusCode,
                    Data = new List<Shift>()
                };
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Shift>>>()
                         ?? new ApiResponseDto<List<Shift>>("Data obtained")
                         {
                             ResponseCode = response.StatusCode,
                             Data = new List<Shift>()
                         };

            _logger.LogInformation("Shifts retrieved successfully from API");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching shifts from API");
            return GetMockShifts(); // Fallback to mock data
        }
    }

    private static ApiResponseDto<List<Shift>> GetMockShifts()
    {
        var shifts = new List<Shift>
        {
            new Shift 
            { 
                ShiftId = 1, 
                WorkerId = 1, 
                LocationId = 1, 
                StartTime = DateTimeOffset.Now.AddHours(-8), 
                EndTime = DateTimeOffset.Now 
            },
            new Shift 
            { 
                ShiftId = 2, 
                WorkerId = 2, 
                LocationId = 2, 
                StartTime = DateTimeOffset.Now.AddHours(-6), 
                EndTime = DateTimeOffset.Now.AddHours(-2) 
            }
        };

        return new ApiResponseDto<List<Shift>>("Success")
        {
            Data = shifts,
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK
        };
    }

    public async Task<ApiResponseDto<Shift?>> GetShiftByIdAsync(int id)
    {
        if (_useMockData)
        {
            return GetMockShiftById(id);
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/shifts/{id}");
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponseDto<Shift>>()
                             ?? new ApiResponseDto<Shift>("No data returned.")
                             {
                                 ResponseCode = response.StatusCode,
                                 Data = null
                             };
                return new ApiResponseDto<Shift?>("Success") 
                { 
                    Data = result.Data,
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK
                };
            }

            string errorMsg = response.StatusCode switch
            {
                HttpStatusCode.NotFound => "Shift not found (404).",
                HttpStatusCode.BadRequest => "Bad request (400) while retrieving shift.",
                HttpStatusCode.InternalServerError => "Server error (500) while retrieving shift.",
                HttpStatusCode.Unauthorized => "Unauthorized (401) while retrieving shift.",
                HttpStatusCode.Forbidden => "Forbidden (403) while retrieving shift.",
                HttpStatusCode.Conflict => "Conflict (409) while retrieving shift.",
                HttpStatusCode.RequestTimeout => "Request Timeout (408) while retrieving shift.",
                (HttpStatusCode)422 => "Unprocessable Entity (422) while retrieving shift.",
                _ => $"Failed to retrieve shift: {response.ReasonPhrase}"
            };
            _logger.LogError(errorMsg);
            return new ApiResponseDto<Shift?>(errorMsg)
            {
                ResponseCode = response.StatusCode,
                RequestFailed = true,
                Data = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching shift {ShiftId} from API", id);
            return GetMockShiftById(id); // Fallback to mock data
        }
    }

    private static ApiResponseDto<Shift?> GetMockShiftById(int id)
    {
        var shift = new Shift 
        { 
            ShiftId = id, 
            WorkerId = 1, 
            LocationId = 1, 
            StartTime = DateTimeOffset.Now.AddHours(-8), 
            EndTime = DateTimeOffset.Now 
        };

        return new ApiResponseDto<Shift?>("Success")
        {
            Data = shift,
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK
        };
    }

    public async Task<ApiResponseDto<Shift>> CreateShiftAsync(Shift shift)
    {
        if (_useMockData)
        {
            return CreateMockShift(shift);
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/shifts", shift);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                string errorMsg = response.StatusCode switch
                {
                    HttpStatusCode.BadRequest => "Bad request (400) while creating shift.",
                    HttpStatusCode.InternalServerError => "Server error (500) while creating shift.",
                    HttpStatusCode.Unauthorized => "Unauthorized (401) while creating shift.",
                    HttpStatusCode.Forbidden => "Forbidden (403) while creating shift.",
                    HttpStatusCode.Conflict => "Conflict (409) while creating shift.",
                    HttpStatusCode.RequestTimeout => "Request Timeout (408) while creating shift.",
                    (HttpStatusCode)422 => "Unprocessable Entity (422) while creating shift.",
                    _ => $"Failed to create shift: {response.ReasonPhrase}"
                };
                _logger.LogError(errorMsg);
                return new ApiResponseDto<Shift>(errorMsg)
                {
                    RequestFailed = true,
                    ResponseCode = response.StatusCode,
                    Data = shift
                };
            }

            var createdShift = await response.Content.ReadFromJsonAsync<Shift>() ?? shift;
            _logger.LogInformation("Shift created successfully");
            
            return new ApiResponseDto<Shift>("Shift created successfully")
            {
                ResponseCode = response.StatusCode,
                RequestFailed = false,
                Data = createdShift
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating shift");
            return CreateMockShift(shift); // Fallback to mock data
        }
    }

    private static ApiResponseDto<Shift> CreateMockShift(Shift shift)
    {
        shift.ShiftId = new Random().Next(1, 1000);

        return new ApiResponseDto<Shift>("Shift created successfully")
        {
            Data = shift,
            RequestFailed = false,
            ResponseCode = HttpStatusCode.Created
        };
    }

    public async Task<ApiResponseDto<Shift?>> UpdateShiftAsync(int id, Shift updatedShift)
    {
    // ...existing code...

        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/shifts/{id}", updatedShift);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string errorMsg = response.StatusCode switch
                {
                    HttpStatusCode.NotFound => "Shift not found (404) for update.",
                    HttpStatusCode.BadRequest => "Bad request (400) while updating shift.",
                    HttpStatusCode.InternalServerError => "Server error (500) while updating shift.",
                    HttpStatusCode.Unauthorized => "Unauthorized (401) while updating shift.",
                    HttpStatusCode.Forbidden => "Forbidden (403) while updating shift.",
                    HttpStatusCode.Conflict => "Conflict (409) while updating shift.",
                    HttpStatusCode.RequestTimeout => "Request Timeout (408) while updating shift.",
                    (HttpStatusCode)422 => "Unprocessable Entity (422) while updating shift.",
                    _ => $"Failed to update shift: {response.ReasonPhrase}"
                };
                _logger.LogError(errorMsg);
                return new ApiResponseDto<Shift?>(errorMsg)
                {
                    ResponseCode = response.StatusCode,
                    RequestFailed = true,
                    Data = null
                };
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponseDto<Shift>>()
                         ?? new ApiResponseDto<Shift>("Update Shift succeeded.")
                         {
                             ResponseCode = response.StatusCode,
                             Data = updatedShift
                         };

            return new ApiResponseDto<Shift?>("Shift updated successfully")
            {
                ResponseCode = response.StatusCode,
                RequestFailed = false,
                Data = result.Data
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating shift {ShiftId}", id);
            return new ApiResponseDto<Shift?>("Error occurred while updating shift")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    // ...existing code...

    public async Task<ApiResponseDto<string?>> DeleteShiftAsync(int id)
    {
    // ...existing code...

        try
        {
            var response = await _httpClient.DeleteAsync($"api/shifts/{id}");
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                string errorMsg = response.StatusCode switch
                {
                    HttpStatusCode.NotFound => "Shift not found (404) for delete.",
                    HttpStatusCode.BadRequest => "Bad request (400) while deleting shift.",
                    HttpStatusCode.InternalServerError => "Server error (500) while deleting shift.",
                    HttpStatusCode.Unauthorized => "Unauthorized (401) while deleting shift.",
                    HttpStatusCode.Forbidden => "Forbidden (403) while deleting shift.",
                    HttpStatusCode.Conflict => "Conflict (409) while deleting shift.",
                    HttpStatusCode.RequestTimeout => "Request Timeout (408) while deleting shift.",
                    (HttpStatusCode)422 => "Unprocessable Entity (422) while deleting shift.",
                    _ => $"Failed to delete shift: {response.ReasonPhrase}"
                };
                _logger.LogError(errorMsg);
                return new ApiResponseDto<string?>(errorMsg)
                {
                    ResponseCode = response.StatusCode,
                    RequestFailed = true,
                    Data = null
                };
            }

            return new ApiResponseDto<string?>("Shift deleted successfully")
            {
                ResponseCode = response.StatusCode,
                RequestFailed = false,
                Data = $"Deleted shift with ID {id}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting shift {ShiftId}", id);
            return new ApiResponseDto<string?>("Error occurred while deleting shift")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }
}
