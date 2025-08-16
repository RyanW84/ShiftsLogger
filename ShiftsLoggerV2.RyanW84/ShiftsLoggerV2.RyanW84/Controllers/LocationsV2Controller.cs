using Microsoft.AspNetCore.Mvc;
using ShiftsLoggerV2.RyanW84.Controllers.Base;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Controllers;

/// <summary>
/// RESTful API controller for Location operations following SOLID principles
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LocationsV2Controller : BaseController<Location, LocationFilterOptions, LocationApiRequestDto, LocationApiRequestDto>
{
    private readonly ILocationBusinessService _locationBusinessService;

    public LocationsV2Controller(ILocationBusinessService locationBusinessService) 
        : base(locationBusinessService)
    {
        _locationBusinessService = locationBusinessService;
    }

    // All CRUD operations are inherited from BaseController
    // Add any location-specific endpoints here if needed
    
    /// <summary>
    /// Get locations by country (example of entity-specific endpoint)
    /// </summary>
    [HttpGet("by-country/{country}")]
    public async Task<ActionResult<ApiResponseDto<List<Location>>>> GetLocationsByCountry(
        string country)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return BadRequest(new ApiResponseDto<List<Location>>
                {
                    RequestFailed = true,
                    ResponseCode = System.Net.HttpStatusCode.BadRequest,
                    Message = "Country is required",
                    Data = null
                });
            }

            var filterOptions = new LocationFilterOptions 
            { 
                Country = country 
            };
            
            var result = await _locationBusinessService.GetAllAsync(filterOptions);
            var response = MapToApiResponse(result);
            
            return result.IsSuccess ? Ok(response) : NotFound(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponseDto<List<Location>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Error retrieving locations by country: {ex.Message}",
                Data = null
            };
            
            return StatusCode(500, response);
        }
    }

    /// <summary>
    /// Get locations by county (example of regional filtering)
    /// </summary>
    [HttpGet("by-county/{county}")]
    public async Task<ActionResult<ApiResponseDto<List<Location>>>> GetLocationsByCounty(
        string county)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(county))
            {
                return BadRequest(new ApiResponseDto<List<Location>>
                {
                    RequestFailed = true,
                    ResponseCode = System.Net.HttpStatusCode.BadRequest,
                    Message = "County is required",
                    Data = null
                });
            }

            var filterOptions = new LocationFilterOptions 
            { 
                County = county 
            };
            
            var result = await _locationBusinessService.GetAllAsync(filterOptions);
            var response = MapToApiResponse(result);
            
            return result.IsSuccess ? Ok(response) : NotFound(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponseDto<List<Location>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Error retrieving locations by county: {ex.Message}",
                Data = null
            };
            
            return StatusCode(500, response);
        }
    }

    /// <summary>
    /// Get unique countries from all locations
    /// </summary>
    [HttpGet("countries")]
    public async Task<ActionResult<ApiResponseDto<List<string>>>> GetUniqueCountries()
    {
        try
        {
            var filterOptions = new LocationFilterOptions();
            var allLocationsResult = await _locationBusinessService.GetAllAsync(filterOptions);
            
            if (allLocationsResult.IsFailure)
            {
                var errorResponse = new ApiResponseDto<List<string>>
                {
                    RequestFailed = true,
                    ResponseCode = allLocationsResult.StatusCode,
                    Message = allLocationsResult.Message,
                    Data = null
                };
                return NotFound(errorResponse);
            }

            var uniqueCountries = allLocationsResult.Data!
                .Select(l => l.Country)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            var response = new ApiResponseDto<List<string>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = $"Found {uniqueCountries.Count} unique countries",
                Data = uniqueCountries
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponseDto<List<string>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Error retrieving unique countries: {ex.Message}",
                Data = null
            };
            
            return StatusCode(500, response);
        }
    }
}
