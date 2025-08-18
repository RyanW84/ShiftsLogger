using System.Net;
using Microsoft.AspNetCore.Mvc;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Services;
using ShiftsLoggerV2.RyanW84.Common;
using Spectre.Console;

namespace ShiftsLoggerV2.RyanW84.Controllers;

[ApiController]
//http://localhost:5009/api/shifts/ this is what the route will look like
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly ShiftValidation _validation;

    public ShiftsController(IShiftService shiftService, ShiftValidation validation)
    {
        _shiftService = shiftService;
        _validation = validation;
    }

    // This is the route for getting all shifts
    [HttpGet(Name = "Get All Shifts")]
    public async Task<ActionResult<ApiResponseDto<List<Shift>>>> GetAllShifts([FromQuery] ShiftFilterOptions shiftOptions)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _validation.GetAllAsync(shiftOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<List<Shift>>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<List<Shift>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Shifts retrieved successfully",
                Data = result.Data?.ToList(),
                TotalCount = result.Data?.Count() ?? 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get All Shifts failed, see Exception {ex}");
            var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<List<Shift>>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            });
        }
    }

    // This is the route for getting a createdShift by ID
    [HttpGet("{id}")] // This will be added to the API URI (send some data during the request
    public async Task<ActionResult<ApiResponseDto<Shift>>> GetShiftById(int id)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _validation.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine($"[red]Shift: {id} Not Found![/]");
                    return NotFound(new ApiResponseDto<Shift>
                    {
                        RequestFailed = true,
                        ResponseCode = System.Net.HttpStatusCode.NotFound,
                        Message = result.Message,
                        Data = null
                    });
                }

                return StatusCode((int)result.StatusCode, new ApiResponseDto<Shift>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            AnsiConsole.MarkupLine($"[Green]Shift: {id} returned successfully[/]");
            return Ok(new ApiResponseDto<Shift>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Shift retrieved successfully",
                Data = result.Data,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get by ID failed, see Exception {ex}");
                var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
                return StatusCode((int)status, new ApiResponseDto<Shift>
                {
                    RequestFailed = true,
                    ResponseCode = status,
                    Message = message,
                    Data = null
                });
        }
    }
    // This is the route for creating a createdShift
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<Shift>>> CreateShift([FromBody] ShiftApiRequestDtoRaw shift)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Parse date/time strings to DateTimeOffset and map to typed DTO
            if (!DateTimeOffset.TryParseExact(shift.StartTime, "dd-MM-yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out var parsedStart))
            {
                ModelState.AddModelError("StartTime", "Invalid date format. Use dd-MM-YYYY HH:mm");
                return BadRequest(ModelState);
            }
            if (!DateTimeOffset.TryParseExact(shift.EndTime, "dd-MM-yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out var parsedEnd))
            {
                ModelState.AddModelError("EndTime", "Invalid date format. Use dd-MM-YYYY HH:mm");
                return BadRequest(ModelState);
            }

            var typedDto = new ShiftApiRequestDto
            {
                WorkerId = shift.WorkerId,
                LocationId = shift.LocationId,
                StartTime = parsedStart,
                EndTime = parsedEnd
            };

            var result = await _validation.CreateAsync(typedDto);

            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<Shift>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return StatusCode(201, new ApiResponseDto<Shift>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.Created,
                Message = "Shift created successfully",
                Data = result.Data,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create shift failed, see Exception {ex}");
                var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
                return StatusCode((int)status, new ApiResponseDto<Shift>
                {
                    RequestFailed = true,
                    ResponseCode = status,
                    Message = message,
                    Data = null
                });
        }
    }

    // This is the route for updating a createdShift
        [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<Shift>>> UpdateShift([FromRoute] int id, [FromBody] ShiftApiRequestDtoRaw shift)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Parse date/time strings to DateTimeOffset and map to typed DTO
            if (!DateTimeOffset.TryParseExact(shift.StartTime, "dd-MM-yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out var parsedStart))
            {
                ModelState.AddModelError("StartTime", "Invalid date format. Use dd-MM-YYYY HH:mm");
                return BadRequest(ModelState);
            }
            if (!DateTimeOffset.TryParseExact(shift.EndTime, "dd-MM-yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out var parsedEnd))
            {
                ModelState.AddModelError("EndTime", "Invalid date format. Use dd-MM-YYYY HH:mm");
                return BadRequest(ModelState);
            }

            var typedDto = new ShiftApiRequestDto
            {
                WorkerId = shift.WorkerId,
                LocationId = shift.LocationId,
                StartTime = parsedStart,
                EndTime = parsedEnd
            };

            var result = await _validation.UpdateAsync(id, typedDto);

            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<Shift>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<Shift>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Shift updated successfully",
                Data = result.Data,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update shift failed, see exception {ex}");
                var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
                return StatusCode((int)status, new ApiResponseDto<Shift>
                {
                    RequestFailed = true,
                    ResponseCode = status,
                    Message = message,
                    Data = null
                });
        }
    }

  
        [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteShift([FromRoute] int id)
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

            return Ok(new ApiResponseDto<object>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Shift deleted successfully",
                Data = null,
                TotalCount = 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete shift failed, see exception {ex}");
                var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
                return StatusCode((int)status, new ApiResponseDto<object>
                {
                    RequestFailed = true,
                    ResponseCode = status,
                    Message = message,
                    Data = null
                });
        }
    }

    // Additional V5 endpoints - Enhanced functionality from SOLID implementation
    
    [HttpGet("by-date-range")]
    public async Task<ActionResult<ApiResponseDto<List<Shift>>>> GetShiftsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var filterOptions = new ShiftFilterOptions
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _validation.GetAllAsync(filterOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<List<Shift>>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<List<Shift>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Shifts retrieved successfully",
                Data = result.Data,
                TotalCount = result.Data?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get shifts by date range failed: {ex}");
                var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
                return StatusCode((int)status, new ApiResponseDto<List<Shift>>
                {
                    RequestFailed = true,
                    ResponseCode = status,
                    Message = message,
                    Data = null
                });
        }
    }

    [HttpGet("worker/{workerId}")]
    public async Task<ActionResult<ApiResponseDto<List<Shift>>>> GetShiftsByWorker(int workerId)
    {
        try
        {
            var filterOptions = new ShiftFilterOptions { WorkerId = workerId };

            var result = await _validation.GetAllAsync(filterOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<List<Shift>>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<List<Shift>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Shifts retrieved successfully",
                Data = result.Data,
                TotalCount = result.Data?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get shifts by worker failed: {ex}");
                var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
                return StatusCode((int)status, new ApiResponseDto<List<Shift>>
                {
                    RequestFailed = true,
                    ResponseCode = status,
                    Message = message,
                    Data = null
                });
        }
    }
}