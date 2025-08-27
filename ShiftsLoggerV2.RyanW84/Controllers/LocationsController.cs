using System.Net;
using Microsoft.AspNetCore.Mvc;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;
using ShiftsLoggerV2.RyanW84.Common;

namespace ShiftsLoggerV2.RyanW84.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : BaseController
{
    private readonly ILocationBusinessService _locationBusinessService;

    public LocationsController(ILocationBusinessService locationBusinessService)
    {
        _locationBusinessService = locationBusinessService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<Location>>>> GetAllLocations([FromQuery] LocationFilterOptions locationOptions)
    {
        try
        {
            var result = await _locationBusinessService.GetAllAsync(locationOptions);
            return HandleResult(result, "Locations retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get All Locations failed, see Exception {ex}");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<List<Location>>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<Location>>> GetLocationById(int id)
    {
        try
        {
            var result = await _locationBusinessService.GetByIdAsync(id);
            return HandleResult(result, "Location retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get Location by ID failed, see Exception {ex}");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<Location>>> CreateLocation([FromBody] LocationApiRequestDto location)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequestModelState();
            }

            var result = await _locationBusinessService.CreateAsync(location);
            return HandleResult(result, "Location created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create Location failed, see Exception {ex}");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message + $" Exception: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<Location>>> UpdateLocation([FromRoute] int id, [FromBody] LocationApiRequestDto updatedLocation)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequestModelState();
            }

            var result = await _locationBusinessService.UpdateAsync(id, updatedLocation);
            return HandleResult(result, "Location updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update Location failed, see Exception {ex}");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message + $" Exception: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<string>>> DeleteLocation(int id)
    {
        try
        {
            var result = await _locationBusinessService.DeleteAsync(id);
            return HandleResult(result, "Location deleted successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete Location failed, see Exception {ex}");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<string>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message + $" Exception: {ex.Message}",
                Data = string.Empty
            });
        }
    }
}
