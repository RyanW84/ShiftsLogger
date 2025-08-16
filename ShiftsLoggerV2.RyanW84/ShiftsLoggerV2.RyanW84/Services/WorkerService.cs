using System.Net;
using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using Spectre.Console;

namespace ShiftsLoggerV2.RyanW84.Services;

public class WorkerService(ShiftsLoggerDbContext dbContext) : IWorkerService
{
    public async Task<ApiResponseDto<List<Worker>>> GetAllWorkers(
        WorkerFilterOptions workerOptions
    )
    {
        var query = dbContext.Workers.AsQueryable<Worker>();

        // Apply all filters
        if (workerOptions.WorkerId != null && workerOptions.WorkerId is not 0)
            query = query.Where(w => w.WorkerId == workerOptions.WorkerId);

        if (!string.IsNullOrWhiteSpace(workerOptions.Name))
            query = query.Where(w => EF.Functions.Like(w.Name, $"%{workerOptions.Name}%"));
        if (!string.IsNullOrWhiteSpace(workerOptions.PhoneNumber))
            query = query.Where(w =>
                EF.Functions.Like(w.PhoneNumber, $"%{workerOptions.PhoneNumber}%")
            );
        if (!string.IsNullOrWhiteSpace(workerOptions.Email))
            query = query.Where(w => EF.Functions.Like(w.Email, $"%{workerOptions.Email}%"));

        // Simplified search implementation
        if (!string.IsNullOrWhiteSpace(workerOptions.Search))
            query = query.Where(w =>
                w.WorkerId.ToString().Contains(workerOptions.Search)
                || EF.Functions.Like(w.Name, $"%{workerOptions.Search}%")
                || EF.Functions.Like(w.PhoneNumber, $"%{workerOptions.Search}%")
                || EF.Functions.Like(w.Email, $"%{workerOptions.Search}%")
            );

        if (!string.IsNullOrWhiteSpace(workerOptions.SortBy))
        {
            workerOptions.SortBy = workerOptions.SortBy.ToLowerInvariant();
            workerOptions.SortOrder = workerOptions.SortOrder?.ToLowerInvariant() ?? "asc"; // Normalize sort order to lowercase with default
        }
        else
        {
            workerOptions.SortBy = "workerid"; // Default sort by WorkerId if not specified
        }

        AnsiConsole.MarkupLine(
            $"[yellow]Applying sorting:[/] SortBy='{workerOptions.SortBy}', SortOrder='{workerOptions.SortOrder}'"
        );

        // Always apply sorting - whether SortBy is specified or not
        query = workerOptions.SortBy switch
        {
            "workerid" => workerOptions.SortOrder == "asc"
                ? query.OrderBy(w => w.WorkerId)
                : query.OrderByDescending(w => w.WorkerId),
            "name" => workerOptions.SortOrder == "asc"
                ? query.OrderBy(w => w.Name)
                : query.OrderByDescending(w => w.Name),
            "phonenumber" => workerOptions.SortOrder == "asc"
                ? query.OrderBy(w => w.PhoneNumber)
                : query.OrderByDescending(w => w.PhoneNumber),
            "email" => workerOptions.SortOrder == "asc"
                ? query.OrderBy(w => w.Email)
                : query.OrderByDescending(w => w.Email),
            _ => query.OrderBy(w => w.WorkerId) // Default sorting by WorkerId
        };

        // Execute query and get results
        var workers = await query.ToListAsync();

        if (workers.Count == 0)
            return new ApiResponseDto<List<Worker>>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.NotFound,
                Message = "No workers found with the specified criteria.",
                Data = workers
            };

        return new ApiResponseDto<List<Worker>>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = "Workers retrieved successfully.",
            Data = workers
        };
    }

    public async Task<ApiResponseDto<Worker>> GetWorkerById(int id)
    {
        var worker = await dbContext.Workers.FirstOrDefaultAsync(w =>
            w.WorkerId == id
        );

        if (worker is null)
            return new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.NotFound,
                Message = "Worker not found.",
                Data = null
            };

        return new ApiResponseDto<Worker>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = "Worker retrieved succesfully",
            Data = worker
        };
    }

    public async Task<ApiResponseDto<Worker>> CreateWorker(WorkerApiRequestDto worker)
    {
        try
        {
            Worker newWorker = new()
            {
                Name = worker.Name,
                PhoneNumber = worker.PhoneNumber,
                Email = worker.Email
            };
            var savedWorker = await dbContext.Workers.AddAsync(newWorker);
            await dbContext.SaveChangesAsync();

            return new ApiResponseDto<Worker>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.Created,
                Message = "Worker created successfully.",
                Data = savedWorker.Entity
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Back end worker service - {ex}");
            return new ApiResponseDto<Worker>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = "An error occurred while creating the worker.",
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Worker?>> UpdateWorker(
        int id,
        WorkerApiRequestDto updatedWorker
    )
    {
        var savedWorker = await dbContext.Workers.FindAsync(id);

        if (savedWorker is null)
            return new ApiResponseDto<Worker?>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.NotFound,
                Message = "Worker not found"
            };
        savedWorker.WorkerId = id; // Ensure the WorkerId is set to the ID being updated
        savedWorker.Name = updatedWorker.Name;
        savedWorker.PhoneNumber = updatedWorker.PhoneNumber;
        savedWorker.Email = updatedWorker.Email;

        dbContext.Workers.Update(savedWorker);
        await dbContext.SaveChangesAsync();

        return new ApiResponseDto<Worker?>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = "Worker updated succesfully",
            Data = savedWorker
        };
    }

    public async Task<ApiResponseDto<string?>> DeleteWorker(int id)
    {
        var savedWorker = await dbContext.Workers.FindAsync(id);

        if (savedWorker is null)
            return new ApiResponseDto<string?>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.NotFound,
                Message = $"Worker with ID: {id} not found.",
                Data = null
            };

        dbContext.Workers.Remove(savedWorker);
        await dbContext.SaveChangesAsync();

        return new ApiResponseDto<string?>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = $"Worker with ID: {id} deleted successfully.",
            Data = null
        };
    }
}