using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;
using ShiftsLoggerV2.RyanW84.Data;
using System.Net;

namespace ShiftsLoggerV2.RyanW84.Core.Repositories;

/// <summary>
/// Base repository implementation with common CRUD operations
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TFilter">Filter options type</typeparam>
/// <typeparam name="TCreateDto">Creation DTO type</typeparam>
/// <typeparam name="TUpdateDto">Update DTO type</typeparam>
public abstract class BaseRepository<TEntity, TFilter, TCreateDto, TUpdateDto> 
    : IRepository<TEntity, TFilter, TCreateDto, TUpdateDto>
    where TEntity : class, IEntity
{
    protected readonly ShiftsLoggerDbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    protected BaseRepository(ShiftsLoggerDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public virtual async Task<Result<List<TEntity>>> GetAllAsync(TFilter filterOptions)
    {
        try
        {
            var query = BuildQuery(filterOptions);
            var entities = await query.ToListAsync();

            if (!entities.Any())
            {
                return Result<List<TEntity>>.NotFound($"No {typeof(TEntity).Name.ToLower()}s found with the specified criteria.");
            }

            return Result<List<TEntity>>.Success(
                entities, 
                $"{typeof(TEntity).Name}s retrieved successfully.", 
                HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            return Result<List<TEntity>>.Failure(
                $"Error retrieving {typeof(TEntity).Name.ToLower()}s: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public virtual async Task<Result<TEntity>> GetByIdAsync(int id)
    {
        try
        {
            var entity = await GetEntityByIdAsync(id);
            
            if (entity == null)
            {
                return Result<TEntity>.NotFound($"{typeof(TEntity).Name} with ID {id} not found.");
            }

            return Result<TEntity>.Success(
                entity,
                $"{typeof(TEntity).Name} retrieved successfully.",
                HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            return Result<TEntity>.Failure(
                $"Error retrieving {typeof(TEntity).Name.ToLower()}: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public virtual async Task<Result<TEntity>> CreateAsync(TCreateDto createDto)
    {
        try
        {
            var entity = await CreateEntityFromDtoAsync(createDto);
            await DbSet.AddAsync(entity);
            await DbContext.SaveChangesAsync();

            return Result<TEntity>.Create(
                entity,
                $"{typeof(TEntity).Name} created successfully."
            );
        }
        catch (Exception ex)
        {
            return Result<TEntity>.Failure(
                $"Error creating {typeof(TEntity).Name.ToLower()}: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public virtual async Task<Result<TEntity>> UpdateAsync(int id, TUpdateDto updateDto)
    {
        try
        {
            var entity = await GetEntityByIdAsync(id);
            
            if (entity == null)
            {
                return Result<TEntity>.NotFound($"{typeof(TEntity).Name} with ID {id} not found.");
            }

            await UpdateEntityFromDtoAsync(entity, updateDto);
            DbSet.Update(entity);
            await DbContext.SaveChangesAsync();

            return Result<TEntity>.Success(
                entity,
                $"{typeof(TEntity).Name} updated successfully.",
                HttpStatusCode.OK
            );
        }
        catch (Exception ex)
        {
            return Result<TEntity>.Failure(
                $"Error updating {typeof(TEntity).Name.ToLower()}: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    public virtual async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var entity = await GetEntityByIdAsync(id);
            
            if (entity == null)
            {
                return Result.NotFound($"{typeof(TEntity).Name} with ID {id} not found.");
            }

            DbSet.Remove(entity);
            await DbContext.SaveChangesAsync();

            return Result.Success(
                $"{typeof(TEntity).Name} deleted successfully.",
                HttpStatusCode.NoContent
            );
        }
        catch (Exception ex)
        {
            return Result.Failure(
                $"Error deleting {typeof(TEntity).Name.ToLower()}: {ex.Message}",
                HttpStatusCode.InternalServerError
            );
        }
    }

    // Abstract methods to be implemented by derived classes
    protected abstract IQueryable<TEntity> BuildQuery(TFilter filterOptions);
    protected abstract Task<TEntity?> GetEntityByIdAsync(int id);
    protected abstract Task<TEntity> CreateEntityFromDtoAsync(TCreateDto createDto);
    protected abstract Task UpdateEntityFromDtoAsync(TEntity entity, TUpdateDto updateDto);
}
