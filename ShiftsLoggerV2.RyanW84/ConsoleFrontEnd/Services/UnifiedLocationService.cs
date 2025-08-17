using System.Net;
using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.Services;

/// <summary>
/// Location Service for API calls - merged and cleaned with comprehensive error handling
/// </summary>
public class UnifiedLocationService : ILocationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UnifiedLocationService> _logger;

    public UnifiedLocationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration, 
        ILogger<UnifiedLocationService> logger)
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

    public async Task<ApiResponseDto<List<Location>>> GetLocationsByFilterAsync(ConsoleFrontEnd.Models.FilterOptions.LocationFilterOptions filter)
    {
        try
        {
            var allResponse = await GetAllLocationsAsync();
            if (allResponse.RequestFailed || allResponse.Data == null)
                return allResponse;

            var filtered = allResponse.Data.AsQueryable();
            if (filter.LocationId.HasValue && filter.LocationId.Value > 0)
                filtered = filtered.Where(l => l.LocationId == filter.LocationId.Value);
            if (!string.IsNullOrWhiteSpace(filter.Name))
                filtered = filtered.Where(l => l.Name != null && l.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(filter.Address))
                filtered = filtered.Where(l => l.Address != null && l.Address.Contains(filter.Address, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(filter.Town))
                filtered = filtered.Where(l => l.Town != null && l.Town.Contains(filter.Town, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(filter.County))
                filtered = filtered.Where(l => l.County != null && l.County.Contains(filter.County, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(filter.PostCode))
                filtered = filtered.Where(l => l.PostCode != null && l.PostCode.Contains(filter.PostCode, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(filter.Country))
                filtered = filtered.Where(l => l.Country != null && l.Country.Contains(filter.Country, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(filter.Search))
                filtered = filtered.Where(l => (l.Name != null && l.Name.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)) ||
                                               (l.Address != null && l.Address.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)) ||
                                               (l.Town != null && l.Town.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)) ||
                                               (l.County != null && l.County.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)) ||
                                               (l.PostCode != null && l.PostCode.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)) ||
                                               (l.Country != null && l.Country.Contains(filter.Search, StringComparison.OrdinalIgnoreCase)));

            var resultList = filtered.ToList();
            return new ApiResponseDto<List<Location>>("Filtered locations")
            {
                Data = resultList,
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                TotalCount = resultList.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering locations");
            return new ApiResponseDto<List<Location>>($"Filter Error: {ex.Message}")
            {
                Data = new List<Location>(),
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError
            };
        }
    }
    
    public async Task<ApiResponseDto<List<Location>>> GetAllLocationsAsync()
    {
        try
        {
            var queryString = "api/locations";
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<Location>>(
                response, 
                _logger, 
                "Get All Locations",
                new List<Location>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching locations from API");
            return new ApiResponseDto<List<Location>>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
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
            return await HttpResponseHelper.HandleHttpResponseAsync<Location?>(
                response,
                _logger,
                $"Get Location {id}",
                null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching location {LocationId}", id);
            return new ApiResponseDto<Location?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Location>> CreateLocationAsync(Location location)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/locations", location);
            return await HttpResponseHelper.HandleHttpResponseAsync<Location>(
                response,
                _logger,
                "Create Location",
                location
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating location");
            return new ApiResponseDto<Location>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Location?>> UpdateLocationAsync(int id, Location updatedLocation)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/locations/{id}", updatedLocation);
            return await HttpResponseHelper.HandleHttpResponseAsync<Location?>(
                response,
                _logger,
                $"Update Location {id}",
                updatedLocation
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating location {LocationId}", id);
            return new ApiResponseDto<Location?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<string?>> DeleteLocationAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/locations/{id}");
            return await HttpResponseHelper.HandleHttpResponseAsync<string?>(
                response,
                _logger,
                $"Delete Location {id}",
                $"Deleted location with ID {id}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting location {LocationId}", id);
            return new ApiResponseDto<string?>($"Connection Error: {ex.Message}")
            {
                ResponseCode = HttpStatusCode.InternalServerError,
                RequestFailed = true,
                Data = null
            };
        }
    }
}
