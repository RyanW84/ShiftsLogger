using Microsoft.AspNetCore.Mvc;
using ShiftsLoggerV2.RyanW84.Controllers.Base;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Controllers;

/// <summary>
/// RESTful API controller for Shift operations following SOLID principles
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ShiftsV2Controller : BaseController<Shift, ShiftFilterOptions, ShiftApiRequestDto, ShiftApiRequestDto>
{
    private readonly IShiftBusinessService _shiftBusinessService;

    public ShiftsV2Controller(IShiftBusinessService shiftBusinessService) 
        : base(shiftBusinessService)
    {
        _shiftBusinessService = shiftBusinessService;
    }

    // All CRUD operations are inherited from BaseController
    // Add any shift-specific endpoints here if needed
    
    /// <summary>
    /// Get shifts by date range (example of entity-specific endpoint)
    /// </summary>
    [HttpGet("by-date-range")]
    public async Task<ActionResult<ApiResponseDto<List<Shift>>>> GetShiftsByDateRange(
        [FromQuery] DateTimeOffset startDate, 
        [FromQuery] DateTimeOffset endDate)
    {
        try
        {
            var filterOptions = new ShiftFilterOptions 
            { 
                StartTime = startDate, 
                EndTime = endDate 
            };
            
            var result = await _shiftBusinessService.GetAllAsync(filterOptions);
            var response = MapToApiResponse(result);
            
            return result.IsSuccess ? Ok(response) : NotFound(response);
        }
        catch (Exception ex)
        {
            return HandleException<List<Shift>>(ex, "Error retrieving shifts by date range");
        }
    }
}
