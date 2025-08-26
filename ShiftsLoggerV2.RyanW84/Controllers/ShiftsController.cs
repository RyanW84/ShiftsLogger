using System.Net;
using Microsoft.AspNetCore.Mvc;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Services;
using ShiftsLoggerV2.RyanW84.Common;
using System.Globalization;
using Spectre.Console;

namespace ShiftsLoggerV2.RyanW84.Controllers;

[ApiController]
//http://localhost:7009/api/shifts/ this is what the route will look like
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

    // Convert ModelState errors into a consistent ApiResponseDto shape
    private ActionResult BadRequestModelState()
    {
        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        var message = errors.Any() ? "Validation failed: " + string.Join("; ", errors) : "Validation failed";
        return BadRequest(new ApiResponseDto<object>
        {
            RequestFailed = true,
            ResponseCode = System.Net.HttpStatusCode.BadRequest,
            Message = message,
            Data = null,
            TotalCount = 0
        });
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
    public async Task<ActionResult<ApiResponseDto<Shift>>> CreateShift([FromBody] ShiftApiRequestDto shift)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"[CreateShift] ModelState invalid: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return BadRequestModelState();
            }

            // Ensure end is after start (JSON converter handles parsing automatically)
            if (shift.EndTime <= shift.StartTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
                return BadRequestModelState();
            }

            var result = await _validation.CreateAsync(shift);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"[CreateShift] Validation failed: {result.Message}");
                return StatusCode((int)result.StatusCode, new ApiResponseDto<Shift>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            Console.WriteLine($"[CreateShift] Shift created successfully for WorkerId={shift.WorkerId}, LocationId={shift.LocationId}, Start={shift.StartTime}, End={shift.EndTime}");
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
            Console.WriteLine($"[CreateShift] Exception: {ex}");
            var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Shift>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message + $" Exception: {ex.Message}",
                Data = null
            });
        }
    }

    // This is the route for updating a createdShift
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<Shift>>> UpdateShift([FromRoute] int id, [FromBody] ShiftApiRequestDto shift)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequestModelState();

            // Ensure end is after start (JSON converter handles parsing automatically)
            if (shift.EndTime <= shift.StartTime)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
                return BadRequestModelState();
            }

            var result = await _validation.UpdateAsync(id, shift);

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
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteShift([FromRoute] int id)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _validation.DeleteAsync(id);

            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<bool>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = false
                });
            }

            return Ok(new ApiResponseDto<bool>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Shift deleted successfully",
                Data = true,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete shift failed, see exception {ex}");
            var (status, message) = ShiftsLoggerV2.RyanW84.Common.ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<bool>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = false
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