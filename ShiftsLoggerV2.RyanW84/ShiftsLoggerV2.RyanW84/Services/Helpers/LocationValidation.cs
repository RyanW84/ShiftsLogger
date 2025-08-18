using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Services.Base;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;
using System.Text.RegularExpressions;

namespace ShiftsLoggerV2.RyanW84.Services;

/// <summary>
/// Business logic service for Location operations
/// </summary>
public class LocationValidation : BaseService<Location, LocationFilterOptions, LocationApiRequestDto, LocationApiRequestDto>, ILocationBusinessService
{
    private readonly ILocationRepository _locationRepository;

    public LocationValidation(ILocationRepository locationRepository) : base(locationRepository)
    {
        _locationRepository = locationRepository;
    }

    protected override Task<Result> ValidateForCreateAsync(LocationApiRequestDto createDto)
    {
        // Business logic validation for location creation
        var validationResult = ValidateLocationData(createDto);
        if (validationResult.IsFailure)
            return Task.FromResult(validationResult);

        return Task.FromResult(Result.Success());
    }

    protected override async Task<Result> ValidateForUpdateAsync(int id, LocationApiRequestDto updateDto)
    {
        // Business logic validation for location updates
        var createValidation = await ValidateForCreateAsync(updateDto);
        if (createValidation.IsFailure)
            return createValidation;

        // Check if location has active shifts before major updates
        var locationResult = await _locationRepository.GetByIdAsync(id);
        if (locationResult.IsFailure)
            return locationResult;

        // Additional update-specific validations could go here
        return Result.Success();
    }

    protected override async Task<Result> ValidateForDeleteAsync(int id)
    {
        // Business logic validation for location deletion
        var locationResult = await _locationRepository.GetByIdAsync(id);
        if (locationResult.IsFailure)
            return locationResult;

        // Check if location has any shifts (you might want to prevent deletion if they have shifts)
        // This would require access to shift repository or a method to check relationships
        // For now, we'll allow deletion but this could be enhanced

        return Result.Success();
    }

    private static Result ValidateLocationData(LocationApiRequestDto dto)
    {
        // Name validation
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result.Failure("Location name is required.");

        if (dto.Name.Length < 2)
            return Result.Failure("Location name must be at least 2 characters long.");

        if (dto.Name.Length > 100)
            return Result.Failure("Location name cannot exceed 100 characters.");

        // Address validation
        if (string.IsNullOrWhiteSpace(dto.Address))
            return Result.Failure("Location address is required.");

        if (dto.Address.Length < 5)
            return Result.Failure("Location address must be at least 5 characters long.");

        if (dto.Address.Length > 200)
            return Result.Failure("Location address cannot exceed 200 characters.");

        // Town validation
        if (string.IsNullOrWhiteSpace(dto.Town))
            return Result.Failure("Location town is required.");

        if (dto.Town.Length < 2)
            return Result.Failure("Location town must be at least 2 characters long.");

        if (dto.Town.Length > 100)
            return Result.Failure("Location town cannot exceed 100 characters.");

        // County validation
        if (string.IsNullOrWhiteSpace(dto.County))
            return Result.Failure("Location county is required.");

        if (dto.County.Length < 2)
            return Result.Failure("Location county must be at least 2 characters long.");

        if (dto.County.Length > 100)
            return Result.Failure("Location county cannot exceed 100 characters.");

        // Post code validation
        if (string.IsNullOrWhiteSpace(dto.PostCode))
            return Result.Failure("Location post code is required.");

        if (!IsValidPostCode(dto.PostCode))
            return Result.Failure("Invalid post code format.");

        if (dto.PostCode.Length > 20)
            return Result.Failure("Location post code cannot exceed 20 characters.");

        // Country validation
        if (string.IsNullOrWhiteSpace(dto.Country))
            return Result.Failure("Location country is required.");

        if (dto.Country.Length < 2)
            return Result.Failure("Location country must be at least 2 characters long.");

        if (dto.Country.Length > 100)
            return Result.Failure("Location country cannot exceed 100 characters.");

        return Result.Success();
    }

    private static bool IsValidPostCode(string postCode)
    {
        if (string.IsNullOrWhiteSpace(postCode))
            return false;

        // Basic post code validation - allows alphanumeric characters, spaces, and dashes
        // This is a simplified pattern that works for most international formats
        var postCodePattern = @"^[A-Za-z0-9\s\-]+$";
        
        try
        {
            return Regex.IsMatch(postCode.Trim(), postCodePattern) && 
                   postCode.Trim().Length >= 3 &&
                   postCode.Any(char.IsLetterOrDigit);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}
