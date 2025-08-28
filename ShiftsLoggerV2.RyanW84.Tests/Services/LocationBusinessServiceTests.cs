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

public class LocationBusinessServiceTests
{
    private readonly Mock<ILocationRepository> _mockLocationRepository;
    private readonly LocationBusinessService _locationBusinessService;

    public LocationBusinessServiceTests()
    {
        _mockLocationRepository = new Mock<ILocationRepository>();
        _locationBusinessService = new LocationBusinessService(_mockLocationRepository.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenSuccessful_ShouldReturnLocations()
    {
        // Arrange
        var filterOptions = new LocationFilterOptions();
        var locations = new List<Location>
        {
            new() { LocationId = 1, Name = "Office A", Address = "123 Main St" },
            new() { LocationId = 2, Name = "Office B", Address = "456 Oak Ave" }
        };

        _mockLocationRepository.Setup(r => r.GetAllAsync(filterOptions))
            .ReturnsAsync(Result<List<Location>>.Success(locations, "Success"));

        // Act
        var result = await _locationBusinessService.GetAllAsync(filterOptions);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].Name.Should().Be("Office A");
        result.Data[1].Name.Should().Be("Office B");
    }

    [Fact]
    public async Task GetByIdAsync_WhenLocationExists_ShouldReturnLocation()
    {
        // Arrange
        const int locationId = 1;
        var location = new Location { LocationId = locationId, Name = "Office A", Address = "123 Main St" };

        _mockLocationRepository.Setup(r => r.GetByIdAsync(locationId))
            .ReturnsAsync(Result<Location>.Success(location, "Location found"));

        // Act
        var result = await _locationBusinessService.GetByIdAsync(locationId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.LocationId.Should().Be(locationId);
        result.Data.Name.Should().Be("Office A");
        result.Data.Address.Should().Be("123 Main St");
    }

    [Fact]
    public async Task GetByIdAsync_WhenLocationNotFound_ShouldReturnFailure()
    {
        // Arrange
        const int locationId = 999;

        _mockLocationRepository.Setup(r => r.GetByIdAsync(locationId))
            .ReturnsAsync(Result<Location>.Failure("Location not found", HttpStatusCode.NotFound));

        // Act
        var result = await _locationBusinessService.GetByIdAsync(locationId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Location not found");
    }

    [Fact]
    public async Task CreateAsync_WithValidLocation_ShouldCreateLocation()
    {
        // Arrange
        var locationDto = new LocationApiRequestDto
        {
            Name = "New Office",
            Address = "789 Pine St",
            Town = "Springfield",
            County = "Test County",
            State = "Test State",
            PostCode = "12345",
            Country = "USA"
        };

        var createdLocation = new Location
        {
            LocationId = 1,
            Name = "New Office",
            Address = "789 Pine St"
        };

        _mockLocationRepository.Setup(r => r.CreateAsync(It.IsAny<LocationApiRequestDto>()))
            .ReturnsAsync(Result<Location>.Success(createdLocation, "Location created successfully"));

        // Act
        var result = await _locationBusinessService.CreateAsync(locationDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("New Office");
        result.Data.Address.Should().Be("789 Pine St");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithInvalidName_ShouldReturnValidationError(string? invalidName)
    {
        // Arrange
        var locationDto = new LocationApiRequestDto
        {
            Name = invalidName!,
            Address = "123 Main St",
            Town = "Springfield"
        };

        // Act
        var result = await _locationBusinessService.CreateAsync(locationDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("Location name is required");
    }

    [Fact]
    public async Task CreateAsync_WithTooLongName_ShouldReturnValidationError()
    {
        // Arrange
        var longName = new string('A', 101); // 101 characters, exceeds max
        var locationDto = new LocationApiRequestDto
        {
            Name = longName,
            Address = "123 Main St",
            Town = "Springfield"
        };

        // Act
        var result = await _locationBusinessService.CreateAsync(locationDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("100 characters");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithInvalidAddress_ShouldReturnValidationError(string? invalidAddress)
    {
        // Arrange
        var locationDto = new LocationApiRequestDto
        {
            Name = "Office A",
            Address = invalidAddress!,
            Town = "Springfield"
        };

        // Act
        var result = await _locationBusinessService.CreateAsync(locationDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("Location address is required");
    }

    [Fact]
    public async Task CreateAsync_WithTooLongAddress_ShouldReturnValidationError()
    {
        // Arrange
        var longAddress = new string('A', 201); // 201 characters, exceeds max
        var locationDto = new LocationApiRequestDto
        {
            Name = "Office A",
            Address = longAddress,
            Town = "Springfield"
        };

        // Act
        var result = await _locationBusinessService.CreateAsync(locationDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("200 characters");
    }

    [Fact]
    public async Task UpdateAsync_WithValidLocation_ShouldUpdateLocation()
    {
        // Arrange
        const int locationId = 1;
        var locationDto = new LocationApiRequestDto
        {
            Name = "Updated Office",
            Address = "999 Updated St",
            Town = "Springfield",
            County = "Test County",
            State = "Test State",
            PostCode = "12345",
            Country = "USA"
        };

        var updatedLocation = new Location
        {
            LocationId = locationId,
            Name = "Updated Office",
            Address = "999 Updated St"
        };

        _mockLocationRepository.Setup(r => r.GetByIdAsync(locationId))
            .ReturnsAsync(Result<Location>.Success(updatedLocation, "Found"));
        _mockLocationRepository.Setup(r => r.UpdateAsync(locationId, It.IsAny<LocationApiRequestDto>()))
            .ReturnsAsync(Result<Location>.Success(updatedLocation, "Location updated successfully"));

        // Act
        var result = await _locationBusinessService.UpdateAsync(locationId, locationDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.LocationId.Should().Be(locationId);
        result.Data.Name.Should().Be("Updated Office");
        result.Data.Address.Should().Be("999 Updated St");
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidName_ShouldReturnValidationError()
    {
        // Arrange
        const int locationId = 1;
        var locationDto = new LocationApiRequestDto
        {
            Name = "",
            Address = "123 Main St",
            Town = "Springfield"
        };

        // Act
        var result = await _locationBusinessService.UpdateAsync(locationId, locationDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("Location name is required");
    }

    [Fact]
    public async Task DeleteAsync_WhenSuccessful_ShouldDeleteLocation()
    {
        // Arrange
        const int locationId = 1;
        var location = new Location { LocationId = locationId, Name = "Test Location" };

        _mockLocationRepository.Setup(r => r.GetByIdAsync(locationId))
            .ReturnsAsync(Result<Location>.Success(location, "Found"));
        _mockLocationRepository.Setup(r => r.HasAssociatedShiftsAsync(locationId))
            .ReturnsAsync(false);
        _mockLocationRepository.Setup(r => r.DeleteAsync(locationId))
            .ReturnsAsync(Result.Success("Location deleted successfully"));

        // Act
        var result = await _locationBusinessService.DeleteAsync(locationId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Location deleted successfully");
    }

    [Fact]
    public async Task DeleteAsync_WhenLocationHasAssociatedShifts_ShouldReturnFailure()
    {
        // Arrange
        const int locationId = 1;
        var location = new Location { LocationId = locationId, Name = "Test Location" };

        _mockLocationRepository.Setup(r => r.GetByIdAsync(locationId))
            .ReturnsAsync(Result<Location>.Success(location, "Found"));
        _mockLocationRepository.Setup(r => r.HasAssociatedShiftsAsync(locationId))
            .ReturnsAsync(true);

        // Act
        var result = await _locationBusinessService.DeleteAsync(locationId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Contain("associated shifts");
    }
}
