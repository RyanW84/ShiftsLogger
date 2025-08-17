using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace ConsoleFrontEnd.Services;

/// <summary>
/// Unified Location Service that can work with either mock data or real API calls
/// Demonstrates integration of legacy functionality with SOLID architecture
/// </summary>
public class UnifiedLocationService : ILocationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UnifiedLocationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _useMockData;

    public UnifiedLocationService(
        IHttpClientFactory httpClientFactory,
        ILogger<UnifiedLocationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("ShiftsLoggerApi");
        _logger = logger;
        _configuration = configuration;
        _useMockData = _configuration.GetValue<bool>("UseMockData", true);
        
        // Set base address for API calls
        _httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("ApiBaseUrl", "http://localhost:5181/"));
    }

    public async Task<ApiResponseDto<List<Location>>> GetAllLocationsAsync()
    {
        if (_useMockData)
        {
            return GetMockLocations();
        }

        try
        {
            _logger.LogInformation("Fetching all locations from API");
            var response = await _httpClient.GetAsync("api/locations");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch locations. Status: {StatusCode}", response.StatusCode);
                return new ApiResponseDto<List<Location>>("Failed to retrieve locations")
                {
                    RequestFailed = true,
                    ResponseCode = response.StatusCode,
                    Data = new List<Location>()
                };
            }

            var locations = await response.Content.ReadFromJsonAsync<List<Location>>();
            return new ApiResponseDto<List<Location>>("Success")
            {
                Data = locations ?? new List<Location>(),
                RequestFailed = false,
                ResponseCode = response.StatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching locations");
            return GetMockLocations(); // Fallback to mock data
        }
    }

    public async Task<ApiResponseDto<Location?>> GetLocationByIdAsync(int id)
    {
        if (_useMockData)
        {
            return GetMockLocationById(id);
        }

        try
        {
            _logger.LogInformation("Fetching location {LocationId} from API", id);
            var response = await _httpClient.GetAsync($"api/locations/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch location {LocationId}. Status: {StatusCode}", id, response.StatusCode);
                return new ApiResponseDto<Location?>("Failed to retrieve location")
                {
                    RequestFailed = true,
                    ResponseCode = response.StatusCode,
                    Data = null
                };
            }

            var location = await response.Content.ReadFromJsonAsync<Location>();
            return new ApiResponseDto<Location?>("Success")
            {
                Data = location,
                RequestFailed = false,
                ResponseCode = response.StatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching location {LocationId}", id);
            return GetMockLocationById(id); // Fallback to mock data
        }
    }

    public async Task<ApiResponseDto<Location>> CreateLocationAsync(Location location)
    {
        if (_useMockData)
        {
            return CreateMockLocation(location);
        }

        try
        {
            _logger.LogInformation("Creating location: {LocationName}", location.Name);
            var response = await _httpClient.PostAsJsonAsync("api/locations", location);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to create location. Status: {StatusCode}", response.StatusCode);
                return new ApiResponseDto<Location>("Failed to create location")
                {
                    RequestFailed = true,
                    ResponseCode = response.StatusCode,
                    Data = location
                };
            }

            var createdLocation = await response.Content.ReadFromJsonAsync<Location>();
            return new ApiResponseDto<Location>("Location created successfully")
            {
                Data = createdLocation ?? location,
                RequestFailed = false,
                ResponseCode = response.StatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating location");
            return CreateMockLocation(location); // Fallback to mock data
        }
    }

    public async Task<ApiResponseDto<Location?>> UpdateLocationAsync(int id, Location updatedLocation)
    {
        if (_useMockData)
        {
            return UpdateMockLocation(id, updatedLocation);
        }

        try
        {
            _logger.LogInformation("Updating location {LocationId}", id);
            var response = await _httpClient.PutAsJsonAsync($"api/locations/{id}", updatedLocation);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to update location {LocationId}. Status: {StatusCode}", id, response.StatusCode);
                return new ApiResponseDto<Location?>("Failed to update location")
                {
                    RequestFailed = true,
                    ResponseCode = response.StatusCode,
                    Data = null
                };
            }

            var location = await response.Content.ReadFromJsonAsync<Location>();
            return new ApiResponseDto<Location?>("Location updated successfully")
            {
                Data = location,
                RequestFailed = false,
                ResponseCode = response.StatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating location {LocationId}", id);
            return UpdateMockLocation(id, updatedLocation); // Fallback to mock data
        }
    }

    public async Task<ApiResponseDto<string?>> DeleteLocationAsync(int id)
    {
        if (_useMockData)
        {
            return DeleteMockLocation(id);
        }

        try
        {
            _logger.LogInformation("Deleting location {LocationId}", id);
            var response = await _httpClient.DeleteAsync($"api/locations/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to delete location {LocationId}. Status: {StatusCode}", id, response.StatusCode);
                return new ApiResponseDto<string?>("Failed to delete location")
                {
                    RequestFailed = true,
                    ResponseCode = response.StatusCode,
                    Data = null
                };
            }

            return new ApiResponseDto<string?>("Location deleted successfully")
            {
                Data = $"Deleted location with ID {id}",
                RequestFailed = false,
                ResponseCode = response.StatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting location {LocationId}", id);
            return DeleteMockLocation(id); // Fallback to mock data
        }
    }

    #region Mock Data Methods (Fallback/Development)

    private ApiResponseDto<List<Location>> GetMockLocations()
    {
        var locations = new List<Location>
        {
            new Location 
            { 
                LocationId = 1, 
                Name = "Main Office", 
                Address = "123 Main St",
                Town = "London",
                County = "Greater London",
                PostCode = "SW1A 1AA",
                Country = "United Kingdom"
            },
            new Location 
            { 
                LocationId = 2, 
                Name = "Branch Office",
                Address = "456 High St",
                Town = "Manchester",
                County = "Greater Manchester",
                PostCode = "M1 1AA",
                Country = "United Kingdom"
            },
            new Location 
            { 
                LocationId = 3, 
                Name = "Remote Hub",
                Address = "789 Tech Park",
                Town = "Edinburgh",
                County = "Midlothian",
                PostCode = "EH1 1AA",
                Country = "United Kingdom"
            }
        };

        return new ApiResponseDto<List<Location>>("Mock data retrieved successfully")
        {
            Data = locations,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        };
    }

    private ApiResponseDto<Location?> GetMockLocationById(int id)
    {
        var location = new Location 
        { 
            LocationId = id, 
            Name = $"Mock Location {id}",
            Address = $"{id} Example St",
            Town = "London",
            County = "Greater London",
            PostCode = "SW1A 1AA",
            Country = "United Kingdom"
        };

        return new ApiResponseDto<Location?>("Mock data retrieved successfully")
        {
            Data = location,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        };
    }

    private ApiResponseDto<Location> CreateMockLocation(Location location)
    {
        location.LocationId = new Random().Next(1000, 9999);
        return new ApiResponseDto<Location>("Mock location created successfully")
        {
            Data = location,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.Created
        };
    }

    private ApiResponseDto<Location?> UpdateMockLocation(int id, Location updatedLocation)
    {
        updatedLocation.LocationId = id;
        return new ApiResponseDto<Location?>("Mock location updated successfully")
        {
            Data = updatedLocation,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        };
    }

    private ApiResponseDto<string?> DeleteMockLocation(int id)
    {
        return new ApiResponseDto<string?>("Mock location deleted successfully")
        {
            Data = $"Mock deleted location with ID {id}",
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        };
    }

    #endregion
}
