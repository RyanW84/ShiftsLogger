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
[Route("api/[controller]")]
public class WorkersController : ControllerBase
{
    private readonly IWorkerService _workerService;
    private readonly WorkerValidation _validation;

    public WorkersController(IWorkerService workerService, WorkerValidation validation)
    {
        _workerService = workerService;
        _validation = validation;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<Worker>>>> GetAllWorkers([FromQuery] WorkerFilterOptions workerOptions)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _validation.GetAllAsync(workerOptions);
            if (!result.IsSuccess)
            {
                AnsiConsole.MarkupLine($"[Red]Error retrieving all workers: {result.Message}[/]");
                return StatusCode((int)result.StatusCode, new ApiResponseDto<List<Worker>>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            AnsiConsole.MarkupLine("[green]Successfully retrieved workers[/]");
            return Ok(new ApiResponseDto<List<Worker>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Workers retrieved successfully",
                Data = result.Data,
                TotalCount = result.Data?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get all workers failed, see Exception {ex}");
            return StatusCode(500, new ApiResponseDto<List<Worker>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Internal server error: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<Worker>>> GetWorkerById(int id)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _validation.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine($"[red]Worker: {id} Not Found![/]");
                    return NotFound(new ApiResponseDto<Worker>
                    {
                        RequestFailed = true,
                        ResponseCode = System.Net.HttpStatusCode.NotFound,
                        Message = result.Message,
                        Data = null
                    });
                }

                return StatusCode((int)result.StatusCode, new ApiResponseDto<Worker>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<Worker>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Worker retrieved successfully",
                Data = result.Data,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get worker by ID failed, see Exception {ex}");
            return StatusCode(500, new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Internal server error: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<Worker>>> CreateWorker([FromBody] WorkerApiRequestDto worker)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Use the new SOLID business service for enhanced functionality
            var result = await _validation.CreateAsync(worker);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<Worker>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return StatusCode(201, new ApiResponseDto<Worker>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.Created,
                Message = "Worker created successfully",
                Data = result.Data,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create worker failed, see Exception {ex}");
            return StatusCode(500, new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Internal server error: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<Worker>>> UpdateWorker(int id, [FromBody] WorkerApiRequestDto updatedWorker)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _validation.UpdateAsync(id, updatedWorker);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<Worker>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<Worker>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Worker updated successfully",
                Data = result.Data,
                TotalCount = 1
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update worker failed, see Exception {ex}");
            return StatusCode(500, new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Internal server error: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteWorker(int id)
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

            Console.WriteLine($"Worker with ID {id} deleted successfully.");
            return Ok(new ApiResponseDto<object>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Worker deleted successfully",
                Data = null,
                TotalCount = 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete worker failed, see Exception {ex}");
            return StatusCode(500, new ApiResponseDto<object>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Internal server error: {ex.Message}",
                Data = null
            });
        }
    }

    // Additional V2 endpoints - Enhanced functionality from SOLID implementation
    
    [HttpGet("by-email-domain")]
    public async Task<ActionResult<ApiResponseDto<List<Worker>>>> GetWorkersByEmailDomain([FromQuery] string domain)
    {
        try
        {
            // Use search to filter by email domain
            var filterOptions = new WorkerFilterOptions { Search = domain };

            var result = await _validation.GetAllAsync(filterOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<List<Worker>>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            // Filter results to only those with the specified domain
            var filteredWorkers = result.Data?.Where(w => !string.IsNullOrEmpty(w.Email) && w.Email.Contains($"@{domain}")).ToList() ?? new List<Worker>();
            return Ok(new ApiResponseDto<List<Worker>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Workers retrieved successfully",
                Data = filteredWorkers,
                TotalCount = filteredWorkers.Count
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get workers by email domain failed: {ex}");
            return StatusCode(500, new ApiResponseDto<List<Worker>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = "Internal server error",
                Data = null
            });
        }
    }

    [HttpGet("by-phone-area-code")]
    public async Task<ActionResult<ApiResponseDto<List<Worker>>>> GetWorkersByPhoneAreaCode([FromQuery] string areaCode)
    {
        try
        {
            // Use phone number filter 
            var filterOptions = new WorkerFilterOptions { PhoneNumber = areaCode };

            var result = await _validation.GetAllAsync(filterOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, new ApiResponseDto<List<Worker>>
                {
                    RequestFailed = true,
                    ResponseCode = result.StatusCode,
                    Message = result.Message,
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<List<Worker>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = "Workers retrieved successfully",
                Data = result.Data,
                TotalCount = result.Data?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get workers by phone area code failed: {ex}");
            return StatusCode(500, new ApiResponseDto<List<Worker>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = "Internal server error",
                Data = null
            });
        }
    }
}
