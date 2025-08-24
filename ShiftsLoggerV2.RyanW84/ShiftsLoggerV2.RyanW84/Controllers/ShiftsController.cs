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
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"[CreateShift] ModelState invalid: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return BadRequest(ModelState);
            }

            // Parse date/time strings to DateTimeOffset and map to typed DTO
            // Accept several formats (dd/MM/yyyy HH:mm, dd-MM-yyyy HH:mm) or standard/ISO formats from clients
            DateTimeOffset parsedStart;
            DateTimeOffset parsedEnd;
            var acceptedFormats = new[] { "dd/MM/yyyy HH:mm", "dd/MM/yyyy H:mm", "dd-MM-yyyy HH:mm", "dd-MM-yyyy H:mm" };

            var startParsed = DateTimeOffset.TryParseExact(shift.StartTime, acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStart)
                              || DateTimeOffset.TryParse(shift.StartTime, out parsedStart);
            if (!startParsed)
            {
                Console.WriteLine($"[CreateShift] Invalid StartTime format: {shift.StartTime}");
                ModelState.AddModelError("StartTime", "Invalid date format. Use dd/MM/yyyy HH:mm, dd-MM-yyyy HH:mm, or ISO date format");
                return BadRequest(ModelState);
            }

            var endParsed = DateTimeOffset.TryParseExact(shift.EndTime, acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEnd)
                            || DateTimeOffset.TryParse(shift.EndTime, out parsedEnd);
            if (!endParsed)
            {
                Console.WriteLine($"[CreateShift] Invalid EndTime format: {shift.EndTime}");
                ModelState.AddModelError("EndTime", "Invalid date format. Use dd/MM/yyyy HH:mm, dd-MM-yyyy HH:mm, or ISO date format");
                return BadRequest(ModelState);
            }

            // Ensure end is after start
            if (parsedEnd <= parsedStart)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
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
                Console.WriteLine($"[CreateShift] Validation failed: {result.Message}");
                return StatusCode((int)result.StatusCode, new ApiResponseDto<Shift>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            Console.WriteLine($"[CreateShift] Shift created successfully for WorkerId={typedDto.WorkerId}, LocationId={typedDto.LocationId}, Start={typedDto.StartTime}, End={typedDto.EndTime}");
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
    public async Task<ActionResult<ApiResponseDto<Shift>>> UpdateShift([FromRoute] int id, [FromBody] ShiftApiRequestDtoRaw shift)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Parse date/time strings to DateTimeOffset and map to typed DTO
            // Accept several formats (dd/MM/yyyy HH:mm, dd-MM-yyyy HH:mm) or standard/ISO formats from clients
            DateTimeOffset parsedStart;
            DateTimeOffset parsedEnd;
            var acceptedFormatsUpdate = new[] { "dd/MM/yyyy HH:mm", "dd/MM/yyyy H:mm", "dd-MM-yyyy HH:mm", "dd-MM-yyyy H:mm" };

            var startParsedUpdate = DateTimeOffset.TryParseExact(shift.StartTime, acceptedFormatsUpdate, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStart)
                                    || DateTimeOffset.TryParse(shift.StartTime, out parsedStart);
            if (!startParsedUpdate)
            {
                ModelState.AddModelError("StartTime", "Invalid date format. Use dd/MM/yyyy HH:mm, dd-MM-yyyy HH:mm, or ISO date format");
                return BadRequest(ModelState);
            }

            var endParsedUpdate = DateTimeOffset.TryParseExact(shift.EndTime, acceptedFormatsUpdate, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEnd)
                                  || DateTimeOffset.TryParse(shift.EndTime, out parsedEnd);
            if (!endParsedUpdate)
            {
                ModelState.AddModelError("EndTime", "Invalid date format. Use dd/MM/yyyy HH:mm, dd-MM-yyyy HH:mm, or ISO date format");
                return BadRequest(ModelState);
            }

            // Ensure end is after start
            if (parsedEnd <= parsedStart)
            {
                ModelState.AddModelError("EndTime", "End time must be after start time.");
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