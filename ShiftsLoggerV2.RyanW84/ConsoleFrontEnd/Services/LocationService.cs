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

    public LocationService(HttpClient httpClient, IConfiguration configuration, ILogger<LocationService> logger)
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

    public async Task<ApiResponseDto<List<Location>>> GetAllLocationsAsync()
    {
        try
        {
            var queryString = "api/locations";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve locations. Status: {StatusCode}", response.StatusCode);
                return new ApiResponseDto<List<Location>>("Failed to retrieve locations")
                {
                    ResponseCode = response.StatusCode,
                    Message = "Failed to retrieve locations",
                    Data = new List<Location>(),
                    RequestFailed = true
                };
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
            return new ApiResponseDto<List<Location>>("Error occurred while fetching locations from API")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = ex.Message,
                Data = new List<Location>(),
                RequestFailed = true
            };
        }
    }



    public async Task<ApiResponseDto<Location?>> GetLocationByIdAsync(int id)
    {
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
            return new ApiResponseDto<Location?>("Error occurred while fetching location from API")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = ex.Message,
                Data = null,
                RequestFailed = true
            };
        }
    }



    public async Task<ApiResponseDto<Location>> CreateLocationAsync(Location location)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/locations", location);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                _logger.LogError("Error creating location. Status Code: {StatusCode}", response.StatusCode);
                return new ApiResponseDto<Location>("Error creating location")
                {
                    ResponseCode = response.StatusCode,
                    RequestFailed = true,
                    Data = null
                };
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
            return new ApiResponseDto<Location>("Error occurred while creating location")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }


    public async Task<ApiResponseDto<Location?>> UpdateLocationAsync(int id, Location updatedLocation)
    {
    // ...existing code...

        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/locations/{id}", updatedLocation);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Error updating location {LocationId}. Status Code: {StatusCode}", id, response.StatusCode);
                return new ApiResponseDto<Location?>("Error updating location")
                {
                    ResponseCode = response.StatusCode,
                    RequestFailed = true,
                    Data = null
                };
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
            return new ApiResponseDto<Location?>("Error occurred while updating location")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    // ...existing code...

    public async Task<ApiResponseDto<string?>> DeleteLocationAsync(int id)
    {
    // ...existing code...

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
