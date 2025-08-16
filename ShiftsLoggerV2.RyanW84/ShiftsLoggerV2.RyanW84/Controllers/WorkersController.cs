using System.Net;
using Microsoft.AspNetCore.Mvc;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Services;
using Spectre.Console;

namespace ShiftsLoggerV2.RyanW84.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkersController(IWorkerService workerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<Worker>>>> GetAllWorkers(
        WorkerFilterOptions workerOptions
    )
    {
        try
        {
            var result = await workerService.GetAllWorkers(workerOptions);
            if (
                result.ResponseCode is HttpStatusCode.NotFound
                || result.ResponseCode is HttpStatusCode.NoContent
            )
            {
                AnsiConsole.MarkupLine(
                    $"[Red]Error retrieving all workers {result.ResponseCode}.[/]"
                );
                return NotFound(
                    new ApiResponseDto<Worker?>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.NotFound,
                        Message = "Error retieving Workers",
                        Data = null
                    }
                );
            }

            AnsiConsole.MarkupLine(
                "[green]Successfully retrieved workers[/]"
            );
            return Ok(result);
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
            var result = await workerService.GetWorkerById(id);
            if (result == null)
            {
                AnsiConsole.MarkupLine(
                    $"[Red]Error retrieving worker: {result.Data.WorkerId}.[/]"
                );
                return NotFound(
                    new ApiResponseDto<Worker?>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.NotFound,
                        Message = $"Failed to retrieve worker by Id {result.ResponseCode}",
                        Data = null
                    }
                );
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Get by ID failed, see Exception {ex}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<Worker>>> CreateWorker(
        WorkerApiRequestDto worker
    )
    {
        try
        {
            var result = await workerService.CreateWorker(worker);
            return CreatedAtAction(
                nameof(GetWorkerById),
                new { id = result.Data.WorkerId },
                result
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Create worker failed, see Exception {ex}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<Worker?>>> UpdateWorker(
        int id,
        WorkerApiRequestDto updatedWorker
    )
    {
        try
        {
            var result = await workerService.UpdateWorker(id, updatedWorker);
            if (result == null || result.Data == null)
            {
                AnsiConsole.MarkupLine(
                    $"[red]Worker not found {result.Data.WorkerId}.[/]"
                );
                return NotFound(
                    new ApiResponseDto<Worker?>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.NotFound,
                        Message = "Worker not found",
                        Data = null
                    }
                );
            }

            AnsiConsole.MarkupLine(
                $"[green]Successfully retrieved worker with ID: {result.Data.WorkerId}.[/]"
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update worker failed, see Exception {ex}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<string>> DeleteWorker(int id)
    {
        try
        {
            var result = await workerService.DeleteWorker(id);
            if (result.ResponseCode is HttpStatusCode.NotFound)
            {
                AnsiConsole.MarkupLine(
                    $"[red]Worker record not found: {id}[/]"
                );
                return NotFound();
            }

            if (result.ResponseCode is HttpStatusCode.NoContent)
            {
                AnsiConsole.MarkupLine(
                    $"[green]Successfully deleted worker with ID: {id}.[/]"
                );
                return NoContent();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete worker failed, see Exception {ex}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}