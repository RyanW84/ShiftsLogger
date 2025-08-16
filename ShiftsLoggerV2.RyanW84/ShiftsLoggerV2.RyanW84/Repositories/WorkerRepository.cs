using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Core.Repositories;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Repositories;

/// <summary>
/// Repository implementation for Worker entity operations
/// </summary>
public class WorkerRepository : BaseRepository<Worker, WorkerFilterOptions, WorkerApiRequestDto, WorkerApiRequestDto>, IWorkerRepository
{
    public WorkerRepository(ShiftsLoggerDbContext dbContext) : base(dbContext)
    {
    }

    protected override IQueryable<Worker> BuildQuery(WorkerFilterOptions filterOptions)
    {
        var query = DbSet.AsQueryable();

        // Apply filters
        if (filterOptions.WorkerId.HasValue && filterOptions.WorkerId.Value > 0)
            query = query.Where(w => w.WorkerId == filterOptions.WorkerId.Value);

        if (!string.IsNullOrEmpty(filterOptions.Name))
            query = query.Where(w => EF.Functions.Like(w.Name, $"%{filterOptions.Name}%"));

        if (!string.IsNullOrEmpty(filterOptions.Email))
            query = query.Where(w => w.Email != null && EF.Functions.Like(w.Email, $"%{filterOptions.Email}%"));

        if (!string.IsNullOrEmpty(filterOptions.PhoneNumber))
            query = query.Where(w => w.PhoneNumber != null && EF.Functions.Like(w.PhoneNumber, $"%{filterOptions.PhoneNumber}%"));

        // Search implementation
        if (!string.IsNullOrWhiteSpace(filterOptions.Search))
            query = query.Where(w =>
                EF.Functions.Like(w.Name, $"%{filterOptions.Search}%") ||
                (w.Email != null && EF.Functions.Like(w.Email, $"%{filterOptions.Search}%")) ||
                (w.PhoneNumber != null && EF.Functions.Like(w.PhoneNumber, $"%{filterOptions.Search}%")) ||
                w.WorkerId.ToString().Contains(filterOptions.Search));

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(filterOptions.SortBy))
        {
            var sortBy = filterOptions.SortBy.ToLowerInvariant();
            var sortOrder = filterOptions.SortOrder?.ToLowerInvariant() ?? "asc";

            query = sortBy switch
            {
                "workerid" => sortOrder == "asc"
                    ? query.OrderBy(w => w.WorkerId)
                    : query.OrderByDescending(w => w.WorkerId),
                "name" => sortOrder == "asc"
                    ? query.OrderBy(w => w.Name)
                    : query.OrderByDescending(w => w.Name),
                "email" => sortOrder == "asc"
                    ? query.OrderBy(w => w.Email ?? "")
                    : query.OrderByDescending(w => w.Email ?? ""),
                "phonenumber" => sortOrder == "asc"
                    ? query.OrderBy(w => w.PhoneNumber ?? "")
                    : query.OrderByDescending(w => w.PhoneNumber ?? ""),
                _ => sortOrder == "asc"
                    ? query.OrderBy(w => w.WorkerId)
                    : query.OrderByDescending(w => w.WorkerId)
            };
        }
        else
        {
            query = query.OrderBy(w => w.WorkerId); // Default sorting
        }

        return query;
    }

    protected override async Task<Worker?> GetEntityByIdAsync(int id)
    {
        return await DbSet.FirstOrDefaultAsync(w => w.WorkerId == id);
    }

    protected override async Task<Worker> CreateEntityFromDtoAsync(WorkerApiRequestDto createDto)
    {
        // Business validation
        if (string.IsNullOrWhiteSpace(createDto.Name))
            throw new ArgumentException("Worker name is required.");

        // Check for duplicate email if provided
        if (!string.IsNullOrWhiteSpace(createDto.Email))
        {
            var emailExists = await DbContext.Workers.AnyAsync(w => w.Email == createDto.Email);
            if (emailExists)
                throw new ArgumentException($"A worker with email {createDto.Email} already exists.");
        }

        return new Worker
        {
            Name = createDto.Name.Trim(),
            Email = string.IsNullOrWhiteSpace(createDto.Email) ? null : createDto.Email.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(createDto.PhoneNumber) ? null : createDto.PhoneNumber.Trim()
        };
    }

    protected override async Task UpdateEntityFromDtoAsync(Worker entity, WorkerApiRequestDto updateDto)
    {
        // Business validation
        if (string.IsNullOrWhiteSpace(updateDto.Name))
            throw new ArgumentException("Worker name is required.");

        // Check for duplicate email if provided and different from current
        if (!string.IsNullOrWhiteSpace(updateDto.Email) && updateDto.Email != entity.Email)
        {
            var emailExists = await DbContext.Workers.AnyAsync(w => w.Email == updateDto.Email && w.WorkerId != entity.WorkerId);
            if (emailExists)
                throw new ArgumentException($"A worker with email {updateDto.Email} already exists.");
        }

        entity.Name = updateDto.Name.Trim();
        entity.Email = string.IsNullOrWhiteSpace(updateDto.Email) ? null : updateDto.Email.Trim();
        entity.PhoneNumber = string.IsNullOrWhiteSpace(updateDto.PhoneNumber) ? null : updateDto.PhoneNumber.Trim();
    }
}
