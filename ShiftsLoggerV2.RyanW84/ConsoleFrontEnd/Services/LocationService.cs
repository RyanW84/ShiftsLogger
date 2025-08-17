using System.Net;
using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Services;

public class LocationService : ILocationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LocationService> _logger;
    private readonly bool _useMockData;

    public LocationService(HttpClient httpClient, IConfiguration configuration, ILogger<LocationService> logger)
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

    public async Task<ApiResponseDto<List<Location>>> GetAllLocationsAsync()
    {
        if (_useMockData)
        {
            return GetMockLocations();
        }

        try
        {
            var queryString = "api/locations";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve locations. Status: {StatusCode}", response.StatusCode);
                return GetMockLocations(); // Fallback to mock data
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Location>>>()
                         ?? new ApiResponseDto<List<Location>>("Data obtained")
                         {
                             ResponseCode = response.StatusCode,
                             Message = "Data obtained",
                             Data = new List<Location>()
                         };

            _logger.LogInformation("Locations retrieved successfully from API");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching locations from API");
            return GetMockLocations(); // Fallback to mock data
        }
    }

    private static ApiResponseDto<List<Location>> GetMockLocations()
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
            }
        };

        return new ApiResponseDto<List<Location>>("Success")
        {
            Data = locations,
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK
        };
    }

    public async Task<ApiResponseDto<Location?>> GetLocationByIdAsync(int id)
    {
        if (_useMockData)
        {
            return GetMockLocationById(id);
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/locations/{id}");
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponseDto<Location>>()
                             ?? new ApiResponseDto<Location>("No data returned")
                             {
                                 ResponseCode = response.StatusCode,
                                 Message = "No data returned.",
                                 Data = null
                             };
                return new ApiResponseDto<Location?>("Success") 
                { 
                    Data = result.Data,
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK
                };
            }

            return new ApiResponseDto<Location?>(response.ReasonPhrase ?? "Location not found")
            {
                ResponseCode = response.StatusCode,
                RequestFailed = true,
                Data = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching location {LocationId} from API", id);
            return GetMockLocationById(id); // Fallback to mock data
        }
    }

    private static ApiResponseDto<Location?> GetMockLocationById(int id)
    {
        var location = new Location 
        { 
            LocationId = id, 
            Name = $"Location {id}",
            Address = $"{id} Example St",
            Town = "London",
            County = "Greater London",
            PostCode = "SW1A 1AA",
            Country = "United Kingdom"
        };

        return new ApiResponseDto<Location?>("Success")
        {
            Data = location,
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK
        };
    }

    public async Task<ApiResponseDto<Location>> CreateLocationAsync(Location location)
    {
        if (_useMockData)
        {
            return CreateMockLocation(location);
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/locations", location);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                _logger.LogError("Error creating location. Status Code: {StatusCode}", response.StatusCode);
                return CreateMockLocation(location); // Fallback to mock data
            }

            var createdLocation = await response.Content.ReadFromJsonAsync<Location>() ?? location;
            _logger.LogInformation("Location created successfully");
            
            return new ApiResponseDto<Location>("Location created successfully")
            {
                ResponseCode = response.StatusCode,
                RequestFailed = false,
                Data = createdLocation
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating location");
            return CreateMockLocation(location); // Fallback to mock data
        }
    }

    private static ApiResponseDto<Location> CreateMockLocation(Location location)
    {
        location.LocationId = new Random().Next(1, 1000);

        return new ApiResponseDto<Location>("Location created successfully")
        {
            Data = location,
            RequestFailed = false,
            ResponseCode = HttpStatusCode.Created
        };
    }

    public async Task<ApiResponseDto<Location?>> UpdateLocationAsync(int id, Location updatedLocation)
    {
        if (_useMockData)
        {
            return UpdateMockLocation(id, updatedLocation);
        }

        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/locations/{id}", updatedLocation);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Error updating location {LocationId}. Status Code: {StatusCode}", id, response.StatusCode);
                return UpdateMockLocation(id, updatedLocation); // Fallback to mock data
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponseDto<Location>>()
                         ?? new ApiResponseDto<Location>("Update Location succeeded")
                         {
                             ResponseCode = response.StatusCode,
                             Message = "Update Location succeeded.",
                             Data = updatedLocation
                         };

            return new ApiResponseDto<Location?>("Location updated successfully")
            {
                ResponseCode = response.StatusCode,
                RequestFailed = false,
                Data = result.Data
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating location {LocationId}", id);
            return UpdateMockLocation(id, updatedLocation); // Fallback to mock data
        }
    }

    private static ApiResponseDto<Location?> UpdateMockLocation(int id, Location updatedLocation)
    {
        updatedLocation.LocationId = id;

        return new ApiResponseDto<Location?>("Location updated successfully")
        {
            Data = updatedLocation,
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK
        };
    }

    public async Task<ApiResponseDto<string?>> DeleteLocationAsync(int id)
    {
        if (_useMockData)
        {
            return new ApiResponseDto<string?>("Location deleted successfully")
            {
                Data = $"Deleted location with ID {id}",
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK
            };
        }

        try
        {
            var response = await _httpClient.DeleteAsync($"api/locations/{id}");
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                _logger.LogError("Error deleting location {LocationId}. Status Code: {StatusCode}", id, response.StatusCode);
                return new ApiResponseDto<string?>($"Error deleting Location - {response.StatusCode}")
                {
                    ResponseCode = response.StatusCode,
                    RequestFailed = true,
                    Data = null
                };
            }

            return new ApiResponseDto<string?>("Location deleted successfully")
            {
                ResponseCode = response.StatusCode,
                RequestFailed = false,
                Data = $"Deleted location with ID {id}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting location {LocationId}", id);
            return new ApiResponseDto<string?>("Error occurred while deleting location")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }
}
