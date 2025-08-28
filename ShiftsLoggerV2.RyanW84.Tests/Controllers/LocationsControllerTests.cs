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

public class LocationsControllerTests
{
    private readonly Mock<ILocationBusinessService> _mockLocationBusinessService;
    private readonly Mock<ILogger<LocationsController>> _mockLogger;
    private readonly LocationsController _controller;

    public LocationsControllerTests()
    {
        _mockLocationBusinessService = new Mock<ILocationBusinessService>();
        _mockLogger = new Mock<ILogger<LocationsController>>();
        _controller = new LocationsController(_mockLocationBusinessService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllLocations_WhenSuccessful_ShouldReturnOkWithLocations()
    {
        // Arrange
        var filterOptions = new LocationFilterOptions();
        var locations = new List<Location>
        {
            new() { LocationId = 1, Name = "Office A", Address = "123 Main St" },
            new() { LocationId = 2, Name = "Office B", Address = "456 Oak Ave" }
        };

        var result = Result<List<Location>>.Success(locations, "Locations retrieved successfully");
        _mockLocationBusinessService.Setup(v => v.GetAllAsync(filterOptions))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetAllLocations(filterOptions);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<PaginatedApiResponseDto<List<Location>>>();

        var apiResponse = okResult.Value as PaginatedApiResponseDto<List<Location>>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.OK);
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data![0].Name.Should().Be("Office A");
        apiResponse.Data[1].Name.Should().Be("Office B");
        apiResponse.TotalCount.Should().Be(2);
        apiResponse.PageNumber.Should().Be(1);
        apiResponse.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllLocations_WhenFailed_ShouldReturnErrorResponse()
    {
        // Arrange
        var filterOptions = new LocationFilterOptions();
        var result = Result<List<Location>>.Failure("Database error", HttpStatusCode.InternalServerError);

        _mockLocationBusinessService.Setup(v => v.GetAllAsync(filterOptions))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetAllLocations(filterOptions);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);

        var apiResponse = objectResult.Value as PaginatedApiResponseDto<List<Location>>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.InternalServerError);
        apiResponse.Message.Should().Be("Database error");
        apiResponse.Data.Should().BeNull();
        apiResponse.TotalCount.Should().Be(0);
        apiResponse.PageNumber.Should().Be(1);
        apiResponse.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetLocationById_WhenLocationExists_ShouldReturnOkWithLocation()
    {
        // Arrange
        const int locationId = 1;
        var location = new Location { LocationId = locationId, Name = "Office A", Address = "123 Main St" };
        var result = Result<Location>.Success(location, "Location found");

        _mockLocationBusinessService.Setup(v => v.GetByIdAsync(locationId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetLocationById(locationId);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;

        var apiResponse = okResult!.Value as ApiResponseDto<Location>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.Data!.LocationId.Should().Be(locationId);
        apiResponse.Data.Name.Should().Be("Office A");
        apiResponse.Data.Address.Should().Be("123 Main St");
    }

    [Fact]
    public async Task GetLocationById_WhenLocationNotFound_ShouldReturnNotFound()
    {
        // Arrange
        const int locationId = 999;
        var result = Result<Location>.Failure("Location not found", HttpStatusCode.NotFound);

        _mockLocationBusinessService.Setup(v => v.GetByIdAsync(locationId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.GetLocationById(locationId);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(404);

        var apiResponse = objectResult.Value as ApiResponseDto<Location>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateLocation_WhenSuccessful_ShouldReturnCreatedWithLocation()
    {
        // Arrange
        var locationDto = new LocationApiRequestDto
        {
            Name = "New Office",
            Address = "789 Pine St"
        };

        var createdLocation = new Location
        {
            LocationId = 1,
            Name = "New Office",
            Address = "789 Pine St"
        };

        var result = Result<Location>.Success(createdLocation, "Location created successfully");
        _mockLocationBusinessService.Setup(v => v.CreateAsync(locationDto))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.CreateLocation(locationDto);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;

        var apiResponse = okResult!.Value as ApiResponseDto<Location>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.Data!.LocationId.Should().Be(1);
        apiResponse.Data.Name.Should().Be("New Office");
        apiResponse.Data.Address.Should().Be("789 Pine St");
    }

    [Fact]
    public async Task CreateLocation_WhenFailed_ShouldReturnErrorResponse()
    {
        // Arrange
        var locationDto = new LocationApiRequestDto
        {
            Name = "New Office",
            Address = "789 Pine St"
        };

        var result = Result<Location>.Failure("Validation error", HttpStatusCode.BadRequest);
        _mockLocationBusinessService.Setup(v => v.CreateAsync(locationDto))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.CreateLocation(locationDto);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;

        var apiResponse = objectResult!.Value as ApiResponseDto<Location>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.BadRequest);
        apiResponse.Message.Should().Be("Validation error");
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateLocation_WhenSuccessful_ShouldReturnOkWithUpdatedLocation()
    {
        // Arrange
        const int locationId = 1;
        var locationDto = new LocationApiRequestDto
        {
            Name = "Updated Office",
            Address = "999 Updated St"
        };

        var updatedLocation = new Location
        {
            LocationId = locationId,
            Name = "Updated Office",
            Address = "999 Updated St"
        };

        var result = Result<Location>.Success(updatedLocation, "Location updated successfully");
        _mockLocationBusinessService.Setup(v => v.UpdateAsync(locationId, locationDto))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.UpdateLocation(locationId, locationDto);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;

        var apiResponse = okResult!.Value as ApiResponseDto<Location>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.Data!.LocationId.Should().Be(locationId);
        apiResponse.Data.Name.Should().Be("Updated Office");
        apiResponse.Data.Address.Should().Be("999 Updated St");
    }

    [Fact]
    public async Task UpdateLocation_WhenFailed_ShouldReturnErrorResponse()
    {
        // Arrange
        const int locationId = 1;
        var locationDto = new LocationApiRequestDto
        {
            Name = "Updated Office",
            Address = "999 Updated St"
        };

        var result = Result<Location>.Failure("Location not found", HttpStatusCode.NotFound);
        _mockLocationBusinessService.Setup(v => v.UpdateAsync(locationId, locationDto))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.UpdateLocation(locationId, locationDto);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;

        var apiResponse = objectResult!.Value as ApiResponseDto<Location>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Message.Should().Be("Location not found");
        apiResponse.Data.Should().BeNull();
    }

    [Fact]
    public async Task DeleteLocation_WhenSuccessful_ShouldReturnOkWithSuccessMessage()
    {
        // Arrange
        const int locationId = 1;
        var result = Result.Success("Location deleted successfully");

        _mockLocationBusinessService.Setup(v => v.DeleteAsync(locationId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.DeleteLocation(locationId);

        // Assert
        response.Result.Should().BeOfType<OkObjectResult>();
        var okResult = response.Result as OkObjectResult;

        var apiResponse = okResult!.Value as ApiResponseDto<string>;
        apiResponse!.RequestFailed.Should().BeFalse();
        apiResponse.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteLocation_WhenFailed_ShouldReturnErrorResponse()
    {
        // Arrange
        const int locationId = 1;
        var result = Result.Failure("Location not found", HttpStatusCode.NotFound);

        _mockLocationBusinessService.Setup(v => v.DeleteAsync(locationId))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.DeleteLocation(locationId);

        // Assert
        response.Result.Should().BeOfType<ObjectResult>();
        var objectResult = response.Result as ObjectResult;

        var apiResponse = objectResult!.Value as ApiResponseDto<string>;
        apiResponse!.RequestFailed.Should().BeTrue();
        apiResponse.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        apiResponse.Message.Should().Be("Location not found");
        apiResponse.Data.Should().BeEmpty();
    }
}
