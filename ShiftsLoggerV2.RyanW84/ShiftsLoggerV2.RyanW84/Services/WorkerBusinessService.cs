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
/// Business logic service for Worker operations
/// </summary>
public class WorkerBusinessService : BaseService<Worker, WorkerFilterOptions, WorkerApiRequestDto, WorkerApiRequestDto>, IWorkerBusinessService
{
    private readonly IWorkerRepository _workerRepository;

    public WorkerBusinessService(IWorkerRepository workerRepository) : base(workerRepository)
    {
        _workerRepository = workerRepository;
    }

    protected override Task<Result> ValidateForCreateAsync(WorkerApiRequestDto createDto)
    {
        // Business logic validation for worker creation
        if (string.IsNullOrWhiteSpace(createDto.Name))
            return Task.FromResult(Result.Failure("Worker name is required."));

        if (createDto.Name.Length < 2)
            return Task.FromResult(Result.Failure("Worker name must be at least 2 characters long."));

        if (createDto.Name.Length > 100)
            return Task.FromResult(Result.Failure("Worker name cannot exceed 100 characters."));

        // Email validation
        if (!string.IsNullOrWhiteSpace(createDto.Email))
        {
            if (!IsValidEmail(createDto.Email))
                return Task.FromResult(Result.Failure("Invalid email format."));

            if (createDto.Email.Length > 254)
                return Task.FromResult(Result.Failure("Email address cannot exceed 254 characters."));
        }

        // Phone number validation
        if (!string.IsNullOrWhiteSpace(createDto.PhoneNumber))
        {
            if (!IsValidPhoneNumber(createDto.PhoneNumber))
                return Task.FromResult(Result.Failure("Invalid phone number format."));

            if (createDto.PhoneNumber.Length > 20)
                return Task.FromResult(Result.Failure("Phone number cannot exceed 20 characters."));
        }

        // At least one contact method required
        if (string.IsNullOrWhiteSpace(createDto.Email) && string.IsNullOrWhiteSpace(createDto.PhoneNumber))
            return Task.FromResult(Result.Failure("At least one contact method (email or phone) is required."));

        return Task.FromResult(Result.Success());
    }

    protected override async Task<Result> ValidateForUpdateAsync(int id, WorkerApiRequestDto updateDto)
    {
        // Business logic validation for worker updates
        var createValidation = await ValidateForCreateAsync(updateDto);
        if (createValidation.IsFailure)
            return createValidation;

        // Check if worker has active shifts before major updates
        var workerResult = await _workerRepository.GetByIdAsync(id);
        if (workerResult.IsFailure)
            return workerResult;

        // Additional update-specific validations could go here
        return Result.Success();
    }

    protected override async Task<Result> ValidateForDeleteAsync(int id)
    {
        // Business logic validation for worker deletion
        var workerResult = await _workerRepository.GetByIdAsync(id);
        if (workerResult.IsFailure)
            return workerResult;

        // Check if worker has any shifts (you might want to prevent deletion if they have shifts)
        // This would require access to shift repository or a method to check relationships
        // For now, we'll allow deletion but this could be enhanced

        return Result.Success();
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Use a simple regex pattern for email validation
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Allow digits, spaces, dashes, parentheses, and plus sign
        var phonePattern = @"^[\d\s\-\(\)\+]+$";
        return Regex.IsMatch(phoneNumber, phonePattern) && phoneNumber.Any(char.IsDigit);
    }
}
