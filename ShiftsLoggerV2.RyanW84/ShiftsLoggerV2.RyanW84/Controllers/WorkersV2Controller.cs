using Microsoft.AspNetCore.Mvc;
using ShiftsLoggerV2.RyanW84.Controllers.Base;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Controllers;

/// <summary>
/// RESTful API controller for Worker operations following SOLID principles
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WorkersV2Controller : BaseController<Worker, WorkerFilterOptions, WorkerApiRequestDto, WorkerApiRequestDto>
{
    private readonly IWorkerBusinessService _workerBusinessService;

    public WorkersV2Controller(IWorkerBusinessService workerBusinessService) 
        : base(workerBusinessService)
    {
        _workerBusinessService = workerBusinessService;
    }

    // All CRUD operations are inherited from BaseController
    // Add any worker-specific endpoints here if needed
    
    /// <summary>
    /// Get workers by email domain (example of entity-specific endpoint)
    /// </summary>
    [HttpGet("by-email-domain")]
    public async Task<ActionResult<ApiResponseDto<List<Worker>>>> GetWorkersByEmailDomain(
        [FromQuery] string domain)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                return BadRequest(new ApiResponseDto<List<Worker>>
                {
                    RequestFailed = true,
                    ResponseCode = System.Net.HttpStatusCode.BadRequest,
                    Message = "Email domain is required",
                    Data = null
                });
            }

            var filterOptions = new WorkerFilterOptions 
            { 
                Search = $"@{domain.TrimStart('@')}" 
            };
            
            var result = await _workerBusinessService.GetAllAsync(filterOptions);
            var response = MapToApiResponse(result);
            
            return result.IsSuccess ? Ok(response) : NotFound(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponseDto<List<Worker>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Error retrieving workers by email domain: {ex.Message}",
                Data = null
            };
            
            return StatusCode(500, response);
        }
    }

    /// <summary>
    /// Get workers with no email (example of business-specific filtering)
    /// </summary>
    [HttpGet("without-email")]
    public async Task<ActionResult<ApiResponseDto<List<Worker>>>> GetWorkersWithoutEmail()
    {
        try
        {
            var filterOptions = new WorkerFilterOptions();
            var allWorkersResult = await _workerBusinessService.GetAllAsync(filterOptions);
            
            if (allWorkersResult.IsFailure)
            {
                var errorResponse = MapToApiResponse(allWorkersResult);
                return NotFound(errorResponse);
            }

            var workersWithoutEmail = allWorkersResult.Data!
                .Where(w => string.IsNullOrWhiteSpace(w.Email))
                .ToList();

            var response = new ApiResponseDto<List<Worker>>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.OK,
                Message = $"Found {workersWithoutEmail.Count} workers without email addresses",
                Data = workersWithoutEmail
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponseDto<List<Worker>>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Error retrieving workers without email: {ex.Message}",
                Data = null
            };
            
            return StatusCode(500, response);
        }
    }
}
