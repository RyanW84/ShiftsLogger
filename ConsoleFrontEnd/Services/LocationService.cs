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
public class LocationService : ILocationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LocationService> _logger;

    public LocationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<LocationService> logger)
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

    public async Task<ApiResponseDto<List<Location>>> GetLocationsByFilterAsync(ConsoleFrontEnd.Models.FilterOptions.LocationFilterOptions filter)
    {
        try
        {
            var queryString = $"api/locations?" + BuildLocationFilterQuery(filter);
            _logger.LogInformation("Making request to: {RequestUrl}", $"{_httpClient.BaseAddress}{queryString}");

            var response = await _httpClient.GetAsync(queryString);
            return await HttpResponseHelper.HandleHttpResponseAsync<List<Location>>(
                response,
                _logger,
                "Get Locations By Filter",
                new List<Location>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering locations via API");
            return new ApiResponseDto<List<Location>>($"Filter Error: {ex.Message}")
            {
                Data = new List<Location>(),
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError
            };
        }
    }

    private string BuildLocationFilterQuery(ConsoleFrontEnd.Models.FilterOptions.LocationFilterOptions filter)
    {
        var query = new List<string>();
        if (filter.LocationId.HasValue) query.Add($"LocationId={filter.LocationId.Value}");
        if (!string.IsNullOrWhiteSpace(filter.Name)) query.Add($"Name={Uri.EscapeDataString(filter.Name)}");
        if (!string.IsNullOrWhiteSpace(filter.Address)) query.Add($"Address={Uri.EscapeDataString(filter.Address)}");
        if (!string.IsNullOrWhiteSpace(filter.Town)) query.Add($"Town={Uri.EscapeDataString(filter.Town)}");
        if (!string.IsNullOrWhiteSpace(filter.County)) query.Add($"County={Uri.EscapeDataString(filter.County)}");
        if (!string.IsNullOrWhiteSpace(filter.PostCode)) query.Add($"PostCode={Uri.EscapeDataString(filter.PostCode)}");
        if (!string.IsNullOrWhiteSpace(filter.Country)) query.Add($"Country={Uri.EscapeDataString(filter.Country)}");
        if (!string.IsNullOrWhiteSpace(filter.Search)) query.Add($"Search={Uri.EscapeDataString(filter.Search)}");
        return string.Join("&", query);
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
        var dto = new ConsoleFrontEnd.Models.Dtos.LocationApiRequestDto
        {
            Name = location.Name,
            Address = location.Address,
            Town = location.Town,
            County = location.County,
            PostCode = location.PostCode,
            Country = location.Country
        };
        var errors = Services.Validation.LocationValidation.Validate(dto);
        if (errors.Count > 0)
        {
            return new ApiResponseDto<Location>("Validation failed")
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                Data = null,
                Message = string.Join("; ", errors)
            };
        }
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/locations", dto);
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
        var dto = new ConsoleFrontEnd.Models.Dtos.LocationApiRequestDto
        {
            Name = updatedLocation.Name,
            Address = updatedLocation.Address,
            Town = updatedLocation.Town,
            County = updatedLocation.County,
            PostCode = updatedLocation.PostCode,
            Country = updatedLocation.Country
        };
        var errors = Services.Validation.LocationValidation.Validate(dto);
        if (errors.Count > 0)
        {
            return new ApiResponseDto<Location?>("Validation failed")
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                Data = null,
                Message = string.Join("; ", errors)
            };
        }
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/locations/{id}", dto);
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
