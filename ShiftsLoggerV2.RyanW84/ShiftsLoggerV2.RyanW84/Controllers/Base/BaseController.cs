using Microsoft.AspNetCore.Mvc;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;
using ShiftsLoggerV2.RyanW84.Dtos;
using System.Net;

namespace ShiftsLoggerV2.RyanW84.Controllers.Base;

/// <summary>
/// Base controller with common CRUD operations following SOLID principles
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TFilter">Filter options type</typeparam>
/// <typeparam name="TCreateDto">Creation DTO type</typeparam>
/// <typeparam name="TUpdateDto">Update DTO type</typeparam>
[ApiController]
public abstract class BaseController<TEntity, TFilter, TCreateDto, TUpdateDto> : ControllerBase
    where TEntity : class, IEntity
{
    protected readonly IService<TEntity, TFilter, TCreateDto, TUpdateDto> Service;
    protected readonly string EntityName;

    protected BaseController(IService<TEntity, TFilter, TCreateDto, TUpdateDto> service)
    {
        Service = service;
        EntityName = typeof(TEntity).Name;
    }

    /// <summary>
    /// Get all entities with filtering
    /// </summary>
    [HttpGet]
    public virtual async Task<ActionResult<ApiResponseDto<List<TEntity>>>> GetAll([FromQuery] TFilter filterOptions)
    {
        try
        {
            var result = await Service.GetAllAsync(filterOptions);
            var response = MapToApiResponse(result);
            
            return result.IsSuccess ? Ok(response) : NotFound(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponseDto<List<TEntity>>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = $"Error retrieving all {EntityName.ToLower()}s: {ex.Message}",
                Data = null
            };
            
            return StatusCode(500, response);
        }
    }

    /// <summary>
    /// Get entity by ID
    /// </summary>
    [HttpGet("{id}")]
    public virtual async Task<ActionResult<ApiResponseDto<TEntity>>> GetById(int id)
    {
        try
        {
            var result = await Service.GetByIdAsync(id);
            var response = MapToApiResponse(result);
            
            return result.IsSuccess ? Ok(response) : NotFound(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponseDto<TEntity>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = $"Error retrieving {EntityName.ToLower()} with ID {id}: {ex.Message}",
                Data = default
            };
            
            return StatusCode(500, response);
        }
    }

    /// <summary>
    /// Create new entity
    /// </summary>
    [HttpPost]
    public virtual async Task<ActionResult<ApiResponseDto<TEntity>>> Create([FromBody] TCreateDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateValidationErrorResponse());

            var result = await Service.CreateAsync(createDto);
            var response = MapToApiResponse(result);
            
            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetById), 
                    new { id = result.Data!.Id }, 
                    response);
            }
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponseDto<TEntity>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = $"Error creating {EntityName.ToLower()}: {ex.Message}",
                Data = default
            };
            
            return StatusCode(500, response);
        }
    }

    /// <summary>
    /// Update existing entity
    /// </summary>
    [HttpPut("{id}")]
    public virtual async Task<ActionResult<ApiResponseDto<TEntity>>> Update(int id, [FromBody] TUpdateDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(CreateValidationErrorResponse());

            var result = await Service.UpdateAsync(id, updateDto);
            var response = MapToApiResponse(result);
            
            return result.IsSuccess ? Ok(response) : NotFound(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponseDto<TEntity>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = $"Error updating {EntityName.ToLower()} with ID {id}: {ex.Message}",
                Data = default
            };
            
            return StatusCode(500, response);
        }
    }

    /// <summary>
    /// Delete entity
    /// </summary>
    [HttpDelete("{id}")]
    public virtual async Task<ActionResult<ApiResponseDto<string>>> Delete(int id)
    {
        try
        {
            var result = await Service.DeleteAsync(id);
            var response = new ApiResponseDto<string>
            {
                RequestFailed = result.IsFailure,
                ResponseCode = result.StatusCode,
                Message = result.Message,
                Data = result.IsSuccess ? "Entity deleted successfully" : null
            };
            
            return result.IsSuccess ? NoContent() : NotFound(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponseDto<string>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = $"Error deleting {EntityName.ToLower()} with ID {id}: {ex.Message}",
                Data = null
            };
            
            return StatusCode(500, response);
        }
    }

    /// <summary>
    /// Map Result to ApiResponseDto
    /// </summary>
    protected virtual ApiResponseDto<T> MapToApiResponse<T>(Result<T> result)
    {
        return new ApiResponseDto<T>
        {
            RequestFailed = result.IsFailure,
            ResponseCode = result.StatusCode,
            Message = result.Message,
            Data = result.Data
        };
    }

    /// <summary>
    /// Handle exceptions consistently
    /// </summary>
    protected virtual ActionResult<ApiResponseDto<T>> HandleException<T>(Exception ex, string message)
    {
        var response = new ApiResponseDto<T>
        {
            RequestFailed = true,
            ResponseCode = HttpStatusCode.InternalServerError,
            Message = $"{message}: {ex.Message}",
            Data = default
        };
        
        // Log the exception here if you have logging configured
        return StatusCode(500, response);
    }

    /// <summary>
    /// Create validation error response
    /// </summary>
    protected virtual ApiResponseDto<TEntity> CreateValidationErrorResponse()
    {
        var errors = ModelState
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();
            
        return new ApiResponseDto<TEntity>
        {
            RequestFailed = true,
            ResponseCode = HttpStatusCode.BadRequest,
            Message = $"Validation failed: {string.Join(", ", errors)}",
            Data = default
        };
    }
}
