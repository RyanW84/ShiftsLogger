using System.Net;
using Microsoft.AspNetCore.Mvc;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Services;
using ShiftsLoggerV2.RyanW84.Common;

namespace ShiftsLoggerV2.RyanW84.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly LocationValidation _validation;

    public LocationsController(ILocationService locationService, LocationValidation validation)
    {
        _locationService = locationService;
        _validation = validation;
    }

    // GET: api/Location
    [HttpGet(Name = "Get All Locations")]
    public async Task<ActionResult<ApiResponseDto<List<Location>>>> GetAllLocations([FromQuery] LocationFilterOptions locationOptions)
    {
        try
        {
            locationOptions ??= new LocationFilterOptions(); // Provide a default value
            
            // Use the new SOLID business service for enhanced functionality
            var result = await _validation.GetAllAsync(locationOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<List<Location>>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<List<Location>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Locations retrieved successfully",
                Data = result.Data,
                TotalCount = result.Data?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get All Locations failed, see Exception {ex}");
            return StatusCode(500, new ApiResponseDto<List<Location>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = "Internal server error",
                Data = null
            });
        }
    }

    // GET: api/Location/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<Location>>> GetLocationById(int id)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _validation.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound(new ApiResponseDto<Location>
                    {
                        RequestFailed = true,
                        ResponseCode = System.Net.HttpStatusCode.NotFound,
                        Message = result.Message,
                        Data = null
                    });
                }

                return StatusCode((int)result.StatusCode, new ApiResponseDto<Location>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<Location>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Location retrieved successfully",
                Data = result.Data,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get location by ID failed, see Exception {ex}");
            return StatusCode(500, new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = "Internal server error",
                Data = null
            });
        }
    }

    // POST: api/Location
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<Location>>> CreateLocation([FromBody] LocationApiRequestDto location)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Use the new SOLID business service for enhanced functionality
            var result = await _validation.CreateAsync(location);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<Location>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return StatusCode(201, new ApiResponseDto<Location>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.Created,
                Message = "Location created successfully",
                Data = result.Data,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create location failed, see Exception {ex}");
            return StatusCode(500, new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = "Internal server error",
                Data = null
            });
        }
    }

    // PUT: api/Location/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<Location>>> UpdateLocation(int id, [FromBody] LocationApiRequestDto updatedLocation)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _validation.UpdateAsync(id, updatedLocation);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<Location>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<Location>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Location updated successfully",
                Data = result.Data,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update location failed, see Exception {ex}");
            return StatusCode(500, new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = "Internal server error",
                Data = null
            });
        }
    }

    // DELETE: api/Location/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteLocation(int id)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _validation.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<object>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            Console.WriteLine($"Location with ID {id} deleted successfully.");
            return Ok(new ApiResponseDto<object>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Location deleted successfully",
                Data = null,
                TotalCount = 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete location failed, see Exception {ex}");
            return StatusCode(500, new ApiResponseDto<object>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = "Internal server error",
                Data = null
            });
        }
    }

    // Additional V2 endpoints - Enhanced functionality from SOLID implementation
    
    [HttpGet("by-country/{country}")]
    public async Task<ActionResult<ApiResponseDto<List<Location>>>> GetLocationsByCountry(string country)
    {
        try
        {
            var filterOptions = new LocationFilterOptions { Country = country };

            var result = await _validation.GetAllAsync(filterOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<List<Location>>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<List<Location>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Locations retrieved successfully",
                Data = result.Data,
                TotalCount = result.Data?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get locations by country failed: {ex}");
            return StatusCode(500, new ApiResponseDto<List<Location>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = "Internal server error",
                Data = null
            });
        }
    }

    [HttpGet("by-county/{county}")]
    public async Task<ActionResult<ApiResponseDto<List<Location>>>> GetLocationsByCounty(string county)
    {
        try
        {
            var filterOptions = new LocationFilterOptions { County = county };

            var result = await _validation.GetAllAsync(filterOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<List<Location>>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<List<Location>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Locations retrieved successfully",
                Data = result.Data,
                TotalCount = result.Data?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get locations by county failed: {ex}");
            return StatusCode(500, new ApiResponseDto<List<Location>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = "Internal server error",
                Data = null
            });
        }
    }
}
