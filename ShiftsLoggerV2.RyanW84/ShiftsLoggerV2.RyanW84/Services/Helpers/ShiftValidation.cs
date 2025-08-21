using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Services.Base;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Services;

/// <summary>
/// Business logic service for Shift operations
/// </summary>
public class ShiftValidation : BaseService<Shift, ShiftFilterOptions, ShiftApiRequestDto, ShiftApiRequestDto>, IShiftBusinessService
{
    private readonly IShiftRepository _shiftRepository;

    public ShiftValidation(IShiftRepository shiftRepository) : base(shiftRepository)
    {
        _shiftRepository = shiftRepository;
    }

    protected override Task<Result> ValidateForCreateAsync(ShiftApiRequestDto createDto)
    {
        // Business logic validation for shift creation
        // Basic required fields
        if (createDto.WorkerId <= 0)
            return Task.FromResult(Result.Failure("WorkerId must be greater than zero."));
        if (createDto.LocationId <= 0)
            return Task.FromResult(Result.Failure("LocationId must be greater than zero."));

        // Start must be before End
        if (createDto.StartTime >= createDto.EndTime)
            return Task.FromResult(Result.Failure("Start time must be before end time."));

        // Allowed date range: +/- 1 year from now
        if (createDto.StartTime < DateTimeOffset.Now.AddYears(-1) || createDto.StartTime > DateTimeOffset.Now.AddYears(1))
            return Task.FromResult(Result.Failure("Start time is out of allowed range."));
        if (createDto.EndTime < DateTimeOffset.Now.AddYears(-1) || createDto.EndTime > DateTimeOffset.Now.AddYears(1))
            return Task.FromResult(Result.Failure("End time is out of allowed range."));

        // Prevent small-past mistakes: don't allow starts more than 5 minutes in the past
        if (createDto.StartTime < DateTimeOffset.Now.AddMinutes(-5))
            return Task.FromResult(Result.Failure("Shift cannot start in the past (with more than 5 minutes tolerance)."));

        var shiftDuration = createDto.EndTime - createDto.StartTime;
        if (shiftDuration.TotalMinutes < 15)
            return Task.FromResult(Result.Failure("Shift duration must be at least 15 minutes."));
        if (shiftDuration.TotalHours > 24)
            return Task.FromResult(Result.Failure("Shift duration cannot exceed 24 hours."));

        return Task.FromResult(Result.Success());
    }

    protected override async Task<Result> ValidateForUpdateAsync(int id, ShiftApiRequestDto updateDto)
    {
        // Business logic validation for shift updates
        var createValidation = await ValidateForCreateAsync(updateDto);
        if (createValidation.IsFailure)
            return createValidation;

        // Additional update-specific validations could go here
        return Result.Success();
    }

    protected override async Task<Result> ValidateForDeleteAsync(int id)
    {
        // Business logic validation for shift deletion
        var shiftResult = await _shiftRepository.GetByIdAsync(id);
        if (shiftResult.IsFailure)
            return shiftResult;

        var shift = shiftResult.Data!;

        // Don't allow deletion of shifts that have already started
        if (shift.StartTime <= DateTimeOffset.Now)
            return Result.Failure("Cannot delete shifts that have already started.");

        return Result.Success();
    }
}
