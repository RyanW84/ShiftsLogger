using FluentAssertions;
using Moq;
using ShiftsLoggerV2.RyanW84.Common;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Services;
using System.Net;
using Xunit;

namespace ShiftsLoggerV2.RyanW84.Tests.Services;

public class ShiftBusinessServiceTests
{
    private readonly Mock<IShiftRepository> _mockShiftRepository;
    private readonly ShiftBusinessService _shiftBusinessService;

    public ShiftBusinessServiceTests()
    {
        _mockShiftRepository = new Mock<IShiftRepository>();
        _shiftBusinessService = new ShiftBusinessService(_mockShiftRepository.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenSuccessful_ShouldReturnShifts()
    {
        // Arrange
        var filterOptions = new ShiftFilterOptions();
        var shifts = new List<Shift>
        {
            new() { ShiftId = 1, WorkerId = 1, LocationId = 1, StartTime = DateTimeOffset.Now, EndTime = DateTimeOffset.Now.AddHours(8) },
            new() { ShiftId = 2, WorkerId = 2, LocationId = 2, StartTime = DateTimeOffset.Now.AddDays(1), EndTime = DateTimeOffset.Now.AddDays(1).AddHours(8) }
        };

        _mockShiftRepository.Setup(r => r.GetAllAsync(filterOptions))
            .ReturnsAsync(Result<List<Shift>>.Success(shifts, "Success"));

        // Act
        var result = await _shiftBusinessService.GetAllAsync(filterOptions);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].ShiftId.Should().Be(1);
        result.Data[1].ShiftId.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenShiftExists_ShouldReturnShift()
    {
        // Arrange
        const int shiftId = 1;
        var shift = new Shift
        {
            ShiftId = shiftId,
            WorkerId = 1,
            LocationId = 1,
            StartTime = DateTimeOffset.Now,
            EndTime = DateTimeOffset.Now.AddHours(8)
        };

        _mockShiftRepository.Setup(r => r.GetByIdAsync(shiftId))
            .ReturnsAsync(Result<Shift>.Success(shift, "Shift found"));

        // Act
        var result = await _shiftBusinessService.GetByIdAsync(shiftId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.ShiftId.Should().Be(shiftId);
        result.Data.WorkerId.Should().Be(1);
        result.Data.LocationId.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_WhenShiftNotFound_ShouldReturnFailure()
    {
        // Arrange
        const int shiftId = 999;

        _mockShiftRepository.Setup(r => r.GetByIdAsync(shiftId))
            .ReturnsAsync(Result<Shift>.Failure("Shift not found", HttpStatusCode.NotFound));

        // Act
        var result = await _shiftBusinessService.GetByIdAsync(shiftId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Shift not found");
    }

    [Fact]
    public async Task CreateAsync_WithValidShift_ShouldCreateShift()
    {
        // Arrange
        var shiftDto = new ShiftApiRequestDto
        {
            WorkerId = 1,
            LocationId = 1,
            StartTime = DateTimeOffset.Now,
            EndTime = DateTimeOffset.Now.AddHours(8)
        };

        var createdShift = new Shift
        {
            ShiftId = 1,
            WorkerId = 1,
            LocationId = 1,
            StartTime = shiftDto.StartTime,
            EndTime = shiftDto.EndTime
        };

        _mockShiftRepository.Setup(r => r.HasOverlappingShiftAsync(shiftDto.WorkerId, shiftDto.LocationId, shiftDto.StartTime, shiftDto.EndTime, null))
            .ReturnsAsync(false);

        _mockShiftRepository.Setup(r => r.CreateAsync(It.IsAny<ShiftApiRequestDto>()))
            .ReturnsAsync(Result<Shift>.Success(createdShift, "Shift created successfully"));

        // Act
        var result = await _shiftBusinessService.CreateAsync(shiftDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.ShiftId.Should().Be(1);
        result.Data.WorkerId.Should().Be(1);
        result.Data.LocationId.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_WithEndTimeBeforeStartTime_ShouldReturnValidationError()
    {
        // Arrange
        var shiftDto = new ShiftApiRequestDto
        {
            WorkerId = 1,
            LocationId = 1,
            StartTime = DateTimeOffset.Now.AddHours(8),
            EndTime = DateTimeOffset.Now // End time before start time
        };

        // Act
        var result = await _shiftBusinessService.CreateAsync(shiftDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("Start time must be before end time");
    }

    [Fact]
    public async Task CreateAsync_WithOverlappingShift_ShouldReturnValidationError()
    {
        // Arrange
        var shiftDto = new ShiftApiRequestDto
        {
            WorkerId = 1,
            LocationId = 1,
            StartTime = DateTimeOffset.Now,
            EndTime = DateTimeOffset.Now.AddHours(8)
        };

        _mockShiftRepository.Setup(r => r.HasOverlappingShiftAsync(shiftDto.WorkerId, shiftDto.LocationId, shiftDto.StartTime, shiftDto.EndTime, null))
            .ReturnsAsync(true);

        // Act
        var result = await _shiftBusinessService.CreateAsync(shiftDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("overlaps an existing shift");
    }

    [Fact]
    public async Task CreateAsync_WithInvalidWorkerId_ShouldReturnValidationError()
    {
        // Arrange
        var shiftDto = new ShiftApiRequestDto
        {
            WorkerId = 0, // Invalid worker ID
            LocationId = 1,
            StartTime = DateTimeOffset.Now,
            EndTime = DateTimeOffset.Now.AddHours(8)
        };

        // Act
        var result = await _shiftBusinessService.CreateAsync(shiftDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("WorkerId must be greater than zero");
    }

    [Fact]
    public async Task CreateAsync_WithInvalidLocationId_ShouldReturnValidationError()
    {
        // Arrange
        var shiftDto = new ShiftApiRequestDto
        {
            WorkerId = 1,
            LocationId = 0, // Invalid location ID
            StartTime = DateTimeOffset.Now,
            EndTime = DateTimeOffset.Now.AddHours(8)
        };

        // Act
        var result = await _shiftBusinessService.CreateAsync(shiftDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("LocationId must be greater than zero");
    }

    [Fact]
    public async Task UpdateAsync_WithValidShift_ShouldUpdateShift()
    {
        // Arrange
        const int shiftId = 1;
        var shiftDto = new ShiftApiRequestDto
        {
            WorkerId = 1,
            LocationId = 2,
            StartTime = DateTimeOffset.Now,
            EndTime = DateTimeOffset.Now.AddHours(8)
        };

        var updatedShift = new Shift
        {
            ShiftId = shiftId,
            WorkerId = 1,
            LocationId = 2,
            StartTime = shiftDto.StartTime,
            EndTime = shiftDto.EndTime
        };

        _mockShiftRepository.Setup(r => r.HasOverlappingShiftAsync(shiftDto.WorkerId, shiftDto.LocationId, shiftDto.StartTime, shiftDto.EndTime, shiftId))
            .ReturnsAsync(false);

        _mockShiftRepository.Setup(r => r.UpdateAsync(shiftId, It.IsAny<ShiftApiRequestDto>()))
            .ReturnsAsync(Result<Shift>.Success(updatedShift, "Shift updated successfully"));

        // Act
        var result = await _shiftBusinessService.UpdateAsync(shiftId, shiftDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.ShiftId.Should().Be(shiftId);
        result.Data.WorkerId.Should().Be(1);
        result.Data.LocationId.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_WithEndTimeBeforeStartTime_ShouldReturnValidationError()
    {
        // Arrange
        const int shiftId = 1;
        var shiftDto = new ShiftApiRequestDto
        {
            WorkerId = 1,
            LocationId = 1,
            StartTime = DateTimeOffset.Now.AddHours(8),
            EndTime = DateTimeOffset.Now // End time before start time
        };

        // Act
        var result = await _shiftBusinessService.UpdateAsync(shiftId, shiftDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("Start time must be before end time");
    }

    [Fact]
    public async Task UpdateAsync_WithOverlappingShift_ShouldReturnValidationError()
    {
        // Arrange
        const int shiftId = 1;
        var shiftDto = new ShiftApiRequestDto
        {
            WorkerId = 1,
            LocationId = 1,
            StartTime = DateTimeOffset.Now,
            EndTime = DateTimeOffset.Now.AddHours(8)
        };

        _mockShiftRepository.Setup(r => r.HasOverlappingShiftAsync(shiftDto.WorkerId, shiftDto.LocationId, shiftDto.StartTime, shiftDto.EndTime, shiftId))
            .ReturnsAsync(true);

        // Act
        var result = await _shiftBusinessService.UpdateAsync(shiftId, shiftDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("would cause an overlap");
    }

    [Fact]
    public async Task DeleteAsync_WhenSuccessful_ShouldDeleteShift()
    {
        // Arrange
        const int shiftId = 1;
        var shift = new Shift
        {
            ShiftId = shiftId,
            WorkerId = 1,
            LocationId = 1,
            StartTime = DateTimeOffset.Now.AddMinutes(-30), // Started 30 minutes ago (within 1 hour limit)
            EndTime = DateTimeOffset.Now.AddMinutes(30) // Ends in 30 minutes
        };

        _mockShiftRepository.Setup(r => r.GetByIdAsync(shiftId))
            .ReturnsAsync(Result<Shift>.Success(shift, "Found"));
        _mockShiftRepository.Setup(r => r.DeleteAsync(shiftId))
            .ReturnsAsync(Result.Success("Shift deleted successfully"));

        // Act
        var result = await _shiftBusinessService.DeleteAsync(shiftId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Shift deleted successfully");
    }

    [Fact]
    public async Task DeleteAsync_WhenShiftNotFound_ShouldReturnFailure()
    {
        // Arrange
        const int shiftId = 1;

        _mockShiftRepository.Setup(r => r.GetByIdAsync(shiftId))
            .ReturnsAsync(Result<Shift>.Failure("Shift not found", HttpStatusCode.NotFound));

        // Act
        var result = await _shiftBusinessService.DeleteAsync(shiftId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Shift not found");
    }
}
