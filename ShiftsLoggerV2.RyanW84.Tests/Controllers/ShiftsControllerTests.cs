using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ShiftsLoggerV2.RyanW84.Controllers;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;
using ShiftsLoggerV2.RyanW84.Common;
using System.Net;
using Xunit;

namespace ShiftsLoggerV2.RyanW84.Tests.Controllers;

public class ShiftsControllerTests
{
    private readonly Mock<IShiftBusinessService> _mockShiftBusinessService;
    private readonly Mock<ILogger<ShiftsController>> _mockLogger;
    private readonly ShiftsController _controller;

    public ShiftsControllerTests()
    {
        _mockShiftBusinessService = new Mock<IShiftBusinessService>();
        _mockLogger = new Mock<ILogger<ShiftsController>>();
        _controller = new ShiftsController(_mockShiftBusinessService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllShifts_WhenSuccessful_ShouldReturnOkWithShifts()
    {
        // Arrange
        var filterOptions = new ShiftFilterOptions();
        var shifts = new List<Shift>
        {
            new() { ShiftId = 1, WorkerId = 1, LocationId = 1, StartTime = DateTimeOffset.Now, EndTime = DateTimeOffset.Now.AddHours(8) },
            new() { ShiftId = 2, WorkerId = 2, LocationId = 2, StartTime = DateTimeOffset.Now.AddDays(1), EndTime = DateTimeOffset.Now.AddDays(1).AddHours(8) }
        };

        var result = Result<List<Shift>>.Success(shifts, "Shifts retrieved successfully");
        _mockShiftBusinessService.Setup(v => v.GetAllAsync(filterOptions))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetAllShifts(filterOptions);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<PaginatedApiResponseDto<List<Shift>>>();

        var apiResponse = okResult.Value as PaginatedApiResponseDto<List<Shift>>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data![0].ShiftId.Should().Be(1);
        apiResponse.Data[1].ShiftId.Should().Be(2);
        apiResponse.TotalCount.Should().Be(2);
        apiResponse.PageNumber.Should().Be(1);
        apiResponse.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllShifts_WhenFailed_ShouldReturnErrorResponse()
    {
        // Arrange
        var filterOptions = new ShiftFilterOptions();
        var result = Result<List<Shift>>.Failure("Database error", HttpStatusCode.InternalServerError);

        _mockShiftBusinessService.Setup(v => v.GetAllAsync(filterOptions))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetAllShifts(filterOptions);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);

        var apiResponse = objectResult.Value as PaginatedApiResponseDto<List<Shift>>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.InternalServerError);
        apiResponse.Message.Should().Be("Database error");
        apiResponse.Data.Should().BeNull();
        apiResponse.TotalCount.Should().Be(0);
        apiResponse.PageNumber.Should().Be(1);
        apiResponse.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetShiftById_WhenShiftExists_ShouldReturnOkWithShift()
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
        var result = Result<Shift>.Success(shift, "Shift found");

        _mockShiftBusinessService.Setup(v => v.GetByIdAsync(shiftId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetShiftById(shiftId);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;

        var apiResponse = okResult!.Value as ApiResponseDto<Shift>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.Data!.ShiftId.Should().Be(shiftId);
        apiResponse.Data.WorkerId.Should().Be(1);
        apiResponse.Data.LocationId.Should().Be(1);
    }

    [Fact]
    public async Task GetShiftById_WhenShiftNotFound_ShouldReturnNotFound()
    {
        // Arrange
        const int shiftId = 999;
        var result = Result<Shift>.Failure("Shift not found", HttpStatusCode.NotFound);

        _mockShiftBusinessService.Setup(v => v.GetByIdAsync(shiftId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetShiftById(shiftId);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(404);

        var apiResponse = objectResult.Value as ApiResponseDto<Shift>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateShift_WhenSuccessful_ShouldReturnCreatedWithShift()
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

        var result = Result<Shift>.Success(createdShift, "Shift created successfully");
        _mockShiftBusinessService.Setup(v => v.CreateAsync(shiftDto))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.CreateShift(shiftDto);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(201);

        var apiResponse = objectResult.Value as ApiResponseDto<Shift>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.Created);
        apiResponse.Data!.ShiftId.Should().Be(1);
        apiResponse.Data.WorkerId.Should().Be(1);
        apiResponse.Data.LocationId.Should().Be(1);
    }

    [Fact]
    public async Task CreateShift_WhenValidationFails_ShouldReturnBadRequest()
    {
        // Arrange
        var shiftDto = new ShiftApiRequestDto
        {
            WorkerId = 1,
            LocationId = 1,
            StartTime = DateTimeOffset.Now.AddHours(8),
            EndTime = DateTimeOffset.Now // End time before start time
        };

        var result = Result<Shift>.Failure("End time must be after start time", HttpStatusCode.BadRequest);
        _mockShiftBusinessService.Setup(v => v.CreateAsync(shiftDto))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.CreateShift(shiftDto);

        // Assert
        response.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().NotBeNull();

        var apiResponse = badRequestResult.Value as ApiResponseDto<Shift>;
        apiResponse.Should().NotBeNull();
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.BadRequest);
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateShift_WhenSuccessful_ShouldReturnOkWithUpdatedShift()
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

        var result = Result<Shift>.Success(updatedShift, "Shift updated successfully");
        _mockShiftBusinessService.Setup(v => v.UpdateAsync(shiftId, shiftDto))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.UpdateShift(shiftId, shiftDto);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;

        var apiResponse = okResult!.Value as ApiResponseDto<Shift>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.Data!.ShiftId.Should().Be(shiftId);
        apiResponse.Data.WorkerId.Should().Be(1);
        apiResponse.Data.LocationId.Should().Be(2);
    }

    [Fact]
    public async Task UpdateShift_WhenValidationFails_ShouldReturnBadRequest()
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

        var result = Result<Shift>.Failure("End time must be after start time", HttpStatusCode.BadRequest);
        _mockShiftBusinessService.Setup(v => v.UpdateAsync(shiftId, shiftDto))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.UpdateShift(shiftId, shiftDto);

        // Assert
        response.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response.Result as BadRequestObjectResult;

        var apiResponse = badRequestResult!.Value as ApiResponseDto<Shift>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.BadRequest);
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task DeleteShift_WhenSuccessful_ShouldReturnOkWithSuccess()
    {
        // Arrange
        const int shiftId = 1;
        var result = Result.Success("Shift deleted successfully");

        _mockShiftBusinessService.Setup(v => v.DeleteAsync(shiftId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.DeleteShift(shiftId);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;

        var apiResponse = okResult!.Value as ApiResponseDto<bool>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.Data.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteShift_WhenFailed_ShouldReturnErrorResponse()
    {
        // Arrange
        const int shiftId = 1;
        var result = Result.Failure("Shift not found", HttpStatusCode.NotFound);

        _mockShiftBusinessService.Setup(v => v.DeleteAsync(shiftId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.DeleteShift(shiftId);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(404);

        var apiResponse = objectResult.Value as ApiResponseDto<bool>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Data.Should().BeFalse();
    }

    [Fact]
    public async Task GetShiftsByDateRange_WhenSuccessful_ShouldReturnOkWithShifts()
    {
        // Arrange
        var startDate = DateTime.Now;
        var endDate = DateTime.Now.AddDays(7);
        var shifts = new List<Shift>
        {
            new() { ShiftId = 1, WorkerId = 1, LocationId = 1, StartTime = DateTimeOffset.Now, EndTime = DateTimeOffset.Now.AddHours(8) }
        };

        var result = Result<List<Shift>>.Success(shifts, "Shifts retrieved successfully");
        _mockShiftBusinessService.Setup(v => v.GetAllAsync(It.Is<ShiftFilterOptions>(fo =>
            fo.StartDate == startDate && fo.EndDate == endDate)))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetShiftsByDateRange(startDate, endDate);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;

        var apiResponse = okResult!.Value as ApiResponseDto<List<Shift>>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.Data.Should().HaveCount(1);
        apiResponse.Data![0].ShiftId.Should().Be(1);
    }

    [Fact]
    public async Task GetShiftsByWorker_WhenSuccessful_ShouldReturnOkWithShifts()
    {
        // Arrange
        const int workerId = 1;
        var shifts = new List<Shift>
        {
            new() { ShiftId = 1, WorkerId = workerId, LocationId = 1, StartTime = DateTimeOffset.Now, EndTime = DateTimeOffset.Now.AddHours(8) }
        };

        var result = Result<List<Shift>>.Success(shifts, "Shifts retrieved successfully");
        _mockShiftBusinessService.Setup(v => v.GetAllAsync(It.Is<ShiftFilterOptions>(fo => fo.WorkerId == workerId)))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetShiftsByWorker(workerId);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;

        var apiResponse = okResult!.Value as ApiResponseDto<List<Shift>>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.Data.Should().HaveCount(1);
        apiResponse.Data![0].WorkerId.Should().Be(workerId);
    }
}
