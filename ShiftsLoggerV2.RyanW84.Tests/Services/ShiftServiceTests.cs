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

public class ShiftServiceTests
{
    private readonly Mock<IShiftRepository> _mockShiftRepository;
    private readonly ShiftService _shiftService;

    public ShiftServiceTests()
    {
        _mockShiftRepository = new Mock<IShiftRepository>();
        _shiftService = new ShiftService(_mockShiftRepository.Object);
    }

    [Fact]
    public async Task GetAllShifts_WhenSuccessful_ShouldReturnSuccessResponse()
    {
        // Arrange
        var filterOptions = new ShiftFilterOptions();
        var shifts = new List<Shift>
        {
            new() { ShiftId = 1, WorkerId = 1, LocationId = 1, StartTime = DateTimeOffset.Now, EndTime = DateTimeOffset.Now.AddHours(8) },
            new() { ShiftId = 2, WorkerId = 2, LocationId = 2, StartTime = DateTimeOffset.Now.AddDays(1), EndTime = DateTimeOffset.Now.AddDays(1).AddHours(8) }
        };

        var repositoryResult = Result<List<Shift>>.Success(shifts, "Success");
        _mockShiftRepository.Setup(r => r.GetAllAsync(filterOptions))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _shiftService.GetAllShifts(filterOptions);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().HaveCount(2);
        result.Data![0].ShiftId.Should().Be(1);
        result.Data[1].ShiftId.Should().Be(2);
    }

    [Fact]
    public async Task GetAllShifts_WhenRepositoryFails_ShouldReturnFailureResponse()
    {
        // Arrange
        var filterOptions = new ShiftFilterOptions();
        var repositoryResult = Result<List<Shift>>.Failure("Database error", HttpStatusCode.InternalServerError);

        _mockShiftRepository.Setup(r => r.GetAllAsync(filterOptions))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _shiftService.GetAllShifts(filterOptions);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Be("Database error");
        result.Data.Should().BeNullOrEmpty();
    }    [Fact]
    public async Task GetShiftById_WhenShiftExists_ShouldReturnShift()
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
        var repositoryResult = Result<Shift>.Success(shift, "Shift found");

        _mockShiftRepository.Setup(r => r.GetByIdAsync(shiftId))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _shiftService.GetShiftById(shiftId);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.OK);
        result.Data!.ShiftId.Should().Be(shiftId);
        result.Data.WorkerId.Should().Be(1);
        result.Data.LocationId.Should().Be(1);
    }

    [Fact]
    public async Task GetShiftById_WhenShiftNotFound_ShouldReturnFailureResponse()
    {
        // Arrange
        const int shiftId = 999;
        var repositoryResult = Result<Shift>.Failure("Shift not found", HttpStatusCode.NotFound);

        _mockShiftRepository.Setup(r => r.GetByIdAsync(shiftId))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _shiftService.GetShiftById(shiftId);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Shift not found");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateShift_WhenSuccessful_ShouldReturnCreatedShift()
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

        var repositoryResult = Result<Shift>.Success(createdShift, "Shift created");
        _mockShiftRepository.Setup(r => r.CreateAsync(shiftDto))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _shiftService.CreateShift(shiftDto);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.OK);
        result.Data!.ShiftId.Should().Be(1);
        result.Data.WorkerId.Should().Be(1);
        result.Data.LocationId.Should().Be(1);
    }

    [Fact]
    public async Task CreateShift_WhenRepositoryFails_ShouldReturnFailureResponse()
    {
        // Arrange
        var shiftDto = new ShiftApiRequestDto
        {
            WorkerId = 1,
            LocationId = 1,
            StartTime = DateTimeOffset.Now,
            EndTime = DateTimeOffset.Now.AddHours(8)
        };

        var repositoryResult = Result<Shift>.Failure("Validation error", HttpStatusCode.BadRequest);
        _mockShiftRepository.Setup(r => r.CreateAsync(shiftDto))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _shiftService.CreateShift(shiftDto);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Be("Validation error");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateShift_WhenSuccessful_ShouldReturnUpdatedShift()
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

        var repositoryResult = Result<Shift>.Success(updatedShift, "Shift updated");
        _mockShiftRepository.Setup(r => r.UpdateAsync(shiftId, shiftDto))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _shiftService.UpdateShift(shiftId, shiftDto);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.OK);
        result.Data!.ShiftId.Should().Be(shiftId);
        result.Data.WorkerId.Should().Be(1);
        result.Data.LocationId.Should().Be(2);
    }

    [Fact]
    public async Task UpdateShift_WhenRepositoryFails_ShouldReturnFailureResponse()
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

        var repositoryResult = Result<Shift>.Failure("Shift not found", HttpStatusCode.NotFound);
        _mockShiftRepository.Setup(r => r.UpdateAsync(shiftId, shiftDto))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _shiftService.UpdateShift(shiftId, shiftDto);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Shift not found");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task DeleteShift_WhenSuccessful_ShouldReturnSuccessResponse()
    {
        // Arrange
        const int shiftId = 1;
        var repositoryResult = Result.Success("Shift deleted");

        _mockShiftRepository.Setup(r => r.DeleteAsync(shiftId))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _shiftService.DeleteShift(shiftId);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("Shift deleted");
    }

    [Fact]
    public async Task DeleteShift_WhenRepositoryFails_ShouldReturnFailureResponse()
    {
        // Arrange
        const int shiftId = 1;
        var repositoryResult = Result.Failure("Shift not found", HttpStatusCode.NotFound);

        _mockShiftRepository.Setup(r => r.DeleteAsync(shiftId))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _shiftService.DeleteShift(shiftId);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Shift not found");
    }
}
