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
    private readonly LocationBusinessService _businessService;

    public LocationsController(ILocationService locationService, LocationBusinessService businessService)
    {
        _locationService = locationService;
        _businessService = businessService;
    }

    // GET: api/Location
    [HttpGet(Name = "Get All Locations")]
    public async Task<ActionResult<List<Location>>> GetAllLocations([FromQuery] LocationFilterOptions locationOptions)
    {
        try
        {
            locationOptions ??= new LocationFilterOptions(); // Provide a default value
            
            // Use the new SOLID business service for enhanced functionality
            var result = await _businessService.GetAllAsync(locationOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get All Locations failed, see Exception {ex}");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Location/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Location>> GetLocationById(int id)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _businessService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound(result.Message);
                }

                return StatusCode((int)result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get location by ID failed, see Exception {ex}");
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Location
    [HttpPost]
    public async Task<ActionResult<Location>> CreateLocation([FromBody] LocationApiRequestDto location)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Use the new SOLID business service for enhanced functionality
            var result = await _businessService.CreateAsync(location);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }

            return StatusCode(201, result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create location failed, see Exception {ex}");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/Location/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<Location>> UpdateLocation(int id, [FromBody] LocationApiRequestDto updatedLocation)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _businessService.UpdateAsync(id, updatedLocation);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update location failed, see Exception {ex}");
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/Location/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteLocation(int id)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _businessService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }

            Console.WriteLine($"Location with ID {id} deleted successfully.");
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete location failed, see Exception {ex}");
            return StatusCode(500, "Internal server error");
        }
    }

    // Additional V2 endpoints - Enhanced functionality from SOLID implementation
    
    [HttpGet("by-country/{country}")]
    public async Task<IActionResult> GetLocationsByCountry(string country)
    {
        try
        {
            var filterOptions = new LocationFilterOptions { Country = country };

            var result = await _businessService.GetAllAsync(filterOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get locations by country failed: {ex}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("by-county/{county}")]
    public async Task<IActionResult> GetLocationsByCounty(string county)
    {
        try
        {
            var filterOptions = new LocationFilterOptions { County = county };

            var result = await _businessService.GetAllAsync(filterOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get locations by county failed: {ex}");
            return StatusCode(500, "Internal server error");
        }
    }
}
