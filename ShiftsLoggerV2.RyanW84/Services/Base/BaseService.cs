using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Services.Base;

/// <summary>
/// Base service implementation that delegates to repository
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TFilter">Filter options type</typeparam>
/// <typeparam name="TCreateDto">Creation DTO type</typeparam>
/// <typeparam name="TUpdateDto">Update DTO type</typeparam>
public abstract class BaseService<TEntity, TFilter, TCreateDto, TUpdateDto> 
    : IService<TEntity, TFilter, TCreateDto, TUpdateDto>
    where TEntity : class, IEntity
{
    protected readonly IRepository<TEntity, TFilter, TCreateDto, TUpdateDto> Repository;

    protected BaseService(IRepository<TEntity, TFilter, TCreateDto, TUpdateDto> repository)
    {
        Repository = repository;
    }

    public virtual async Task<Result<List<TEntity>>> GetAllAsync(TFilter filterOptions)
    {
        // Add any business logic validation here if needed
        return await Repository.GetAllAsync(filterOptions);
    }

    public virtual async Task<Result<TEntity>> GetByIdAsync(int id)
    {
        // Add any business logic validation here if needed
        if (id <= 0)
            return Result<TEntity>.Failure("ID must be greater than zero.");

        return await Repository.GetByIdAsync(id);
    }

    public virtual async Task<Result<TEntity>> CreateAsync(TCreateDto createDto)
    {
        // Add any business logic validation here if needed
        var validationResult = await ValidateForCreateAsync(createDto);
        if (validationResult.IsFailure)
            return Result<TEntity>.Failure(validationResult.Message);

        return await Repository.CreateAsync(createDto);
    }

    public virtual async Task<Result<TEntity>> UpdateAsync(int id, TUpdateDto updateDto)
    {
        // Add any business logic validation here if needed
        if (id <= 0)
            return Result<TEntity>.Failure("ID must be greater than zero.");

        var validationResult = await ValidateForUpdateAsync(id, updateDto);
        if (validationResult.IsFailure)
            return Result<TEntity>.Failure(validationResult.Message);

        return await Repository.UpdateAsync(id, updateDto);
    }

    public virtual async Task<Result> DeleteAsync(int id)
    {
        // Add any business logic validation here if needed
        if (id <= 0)
            return Result.Failure("ID must be greater than zero.");

        var validationResult = await ValidateForDeleteAsync(id);
        if (validationResult.IsFailure)
            return Result.Failure(validationResult.Message);

        return await Repository.DeleteAsync(id);
    }

    // Virtual methods for business logic validation - can be overridden by derived classes
    protected virtual async Task<Result> ValidateForCreateAsync(TCreateDto createDto)
    {
        await Task.CompletedTask; // Placeholder for async consistency
        return Result.Success();
    }

    protected virtual async Task<Result> ValidateForUpdateAsync(int id, TUpdateDto updateDto)
    {
        await Task.CompletedTask; // Placeholder for async consistency
        return Result.Success();
    }

    protected virtual async Task<Result> ValidateForDeleteAsync(int id)
    {
        await Task.CompletedTask; // Placeholder for async consistency
        return Result.Success();
    }
}
