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
    private readonly WorkerBusinessService _businessService;

    public WorkersController(IWorkerService workerService, WorkerBusinessService businessService)
    {
        _workerService = workerService;
        _businessService = businessService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Worker>>> GetAllWorkers([FromQuery] WorkerFilterOptions workerOptions)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _businessService.GetAllAsync(workerOptions);
            if (!result.IsSuccess)
            {
                AnsiConsole.MarkupLine($"[Red]Error retrieving all workers: {result.Message}[/]");
                return StatusCode((int)result.StatusCode, result.Message);
            }

            AnsiConsole.MarkupLine("[green]Successfully retrieved workers[/]");
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get all workers failed, see Exception {ex}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Worker>> GetWorkerById(int id)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _businessService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine($"[red]Worker: {id} Not Found![/]");
                    return NotFound(result.Message);
                }

                return StatusCode((int)result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get worker by ID failed, see Exception {ex}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Worker>> CreateWorker([FromBody] WorkerApiRequestDto worker)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Use the new SOLID business service for enhanced functionality
            var result = await _businessService.CreateAsync(worker);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }

            return StatusCode(201, result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create worker failed, see Exception {ex}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Worker>> UpdateWorker(int id, [FromBody] WorkerApiRequestDto updatedWorker)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _businessService.UpdateAsync(id, updatedWorker);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update worker failed, see Exception {ex}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWorker(int id)
    {
        try
        {
            // Use the new SOLID business service for enhanced functionality
            var result = await _businessService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }

            Console.WriteLine($"Worker with ID {id} deleted successfully.");
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete worker failed, see Exception {ex}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // Additional V2 endpoints - Enhanced functionality from SOLID implementation
    
    [HttpGet("by-email-domain")]
    public async Task<IActionResult> GetWorkersByEmailDomain([FromQuery] string domain)
    {
        try
        {
            var filterOptions = new WorkerFilterOptions { EmailDomain = domain };

            var result = await _businessService.GetAllAsync(filterOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get workers by email domain failed: {ex}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("by-phone-area-code")]
    public async Task<IActionResult> GetWorkersByPhoneAreaCode([FromQuery] string areaCode)
    {
        try
        {
            var filterOptions = new WorkerFilterOptions { PhonePrefix = areaCode };

            var result = await _businessService.GetAllAsync(filterOptions);
            if (!result.IsSuccess)
            {
                return StatusCode((int)result.StatusCode, result.Message);
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get workers by phone area code failed: {ex}");
            return StatusCode(500, "Internal server error");
        }
    }
}
