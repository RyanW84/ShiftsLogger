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
        if (createDto.StartTime >= createDto.EndTime)
            return Task.FromResult(Result.Failure("Start time must be before end time."));

        if (createDto.StartTime < DateTimeOffset.Now.AddDays(-14))
            return Task.FromResult(Result.Failure("Cannot create shifts more than 14 days in the past."));

        if (createDto.StartTime > DateTimeOffset.Now.AddDays(90))
            return Task.FromResult(Result.Failure("Cannot create shifts more than 90 days in the future."));

        var shiftDuration = createDto.EndTime - createDto.StartTime;
        if (shiftDuration.TotalHours > 24)
            return Task.FromResult(Result.Failure("Shift duration cannot exceed 24 hours."));

        if (shiftDuration.TotalMinutes < 15)
            return Task.FromResult(Result.Failure("Shift duration must be at least 15 minutes."));

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
