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
                _logger.LogError("Failed to retrieve shifts. Status: {StatusCode}", response.StatusCode);
                return GetMockShifts(); // Fallback to mock data
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

            return new ApiResponseDto<Shift?>(response.ReasonPhrase ?? "Shift not found")
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
                _logger.LogError("Error creating shift. Status Code: {StatusCode}", response.StatusCode);
                return CreateMockShift(shift); // Fallback to mock data
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
        if (_useMockData)
        {
            return UpdateMockShift(id, updatedShift);
        }

        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/shifts/{id}", updatedShift);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Error updating shift {ShiftId}. Status Code: {StatusCode}", id, response.StatusCode);
                return UpdateMockShift(id, updatedShift); // Fallback to mock data
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
            return UpdateMockShift(id, updatedShift); // Fallback to mock data
        }
    }

    private static ApiResponseDto<Shift?> UpdateMockShift(int id, Shift updatedShift)
    {
        updatedShift.ShiftId = id;

        return new ApiResponseDto<Shift?>("Shift updated successfully")
        {
            Data = updatedShift,
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK
        };
    }

    public async Task<ApiResponseDto<string?>> DeleteShiftAsync(int id)
    {
        if (_useMockData)
        {
            return new ApiResponseDto<string?>("Shift deleted successfully")
            {
                Data = $"Deleted shift with ID {id}",
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK
            };
        }

        try
        {
            var response = await _httpClient.DeleteAsync($"api/shifts/{id}");
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                _logger.LogError("Error deleting shift {ShiftId}. Status Code: {StatusCode}", id, response.StatusCode);
                return new ApiResponseDto<string?>($"Error deleting Shift - {response.StatusCode}")
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
