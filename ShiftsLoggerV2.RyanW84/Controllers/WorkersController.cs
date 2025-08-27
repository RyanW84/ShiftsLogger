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
public class WorkersController : BaseController
{
    private readonly IWorkerBusinessService _workerBusinessService;

    public WorkersController(IWorkerBusinessService workerBusinessService)
    {
        _workerBusinessService = workerBusinessService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<Worker>>>> GetAllWorkers([FromQuery] WorkerFilterOptions workerOptions)
    {
        try
        {
            var result = await _workerBusinessService.GetAllAsync(workerOptions);
            return HandleResult(result, "Workers retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get All Workers failed, see Exception {ex}");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<List<Worker>>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<Worker>>> GetWorkerById(int id)
    {
        try
        {
            var result = await _workerBusinessService.GetByIdAsync(id);
            return HandleResult(result, "Worker retrieved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get Worker by ID failed, see Exception {ex}");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message,
                Data = null
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<Worker>>> CreateWorker([FromBody] WorkerApiRequestDto worker)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequestModelState();
            }

            var result = await _workerBusinessService.CreateAsync(worker);
            return HandleResult(result, "Worker created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create Worker failed, see Exception {ex}");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message + $" Exception: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<Worker>>> UpdateWorker([FromRoute] int id, [FromBody] WorkerApiRequestDto updatedWorker)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequestModelState();
            }

            var result = await _workerBusinessService.UpdateAsync(id, updatedWorker);
            return HandleResult(result, "Worker updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update Worker failed, see Exception {ex}");
            var (status, message) = ErrorMapper.Map(ex);
            return StatusCode((int)status, new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = status,
                Message = message + $" Exception: {ex.Message}",
                Data = null
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<string>>> DeleteWorker(int id)
    {
        try
        {
            var result = await _workerBusinessService.DeleteAsync(id);
            return HandleResult(result, "Worker deleted successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete Worker failed, see Exception {ex}");
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
