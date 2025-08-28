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

public class LocationServiceTests
{
    private readonly Mock<ILocationRepository> _mockLocationRepository;
    private readonly LocationService _locationService;

    public LocationServiceTests()
    {
        _mockLocationRepository = new Mock<ILocationRepository>();
        _locationService = new LocationService(_mockLocationRepository.Object);
    }

    [Fact]
    public async Task GetAllLocations_WhenSuccessful_ShouldReturnSuccessResponse()
    {
        // Arrange
        var filterOptions = new LocationFilterOptions();
        var locations = new List<Location>
        {
            new() { LocationId = 1, Name = "Office A", Address = "123 Main St" },
            new() { LocationId = 2, Name = "Office B", Address = "456 Oak Ave" }
        };

        var repositoryResult = Result<List<Location>>.Success(locations, "Success");
        _mockLocationRepository.Setup(r => r.GetAllAsync(filterOptions))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _locationService.GetAllLocations(filterOptions);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().HaveCount(2);
        result.Data![0].Name.Should().Be("Office A");
        result.Data[1].Name.Should().Be("Office B");
    }

    [Fact]
    public async Task GetAllLocations_WhenRepositoryFails_ShouldReturnFailureResponse()
    {
        // Arrange
        var filterOptions = new LocationFilterOptions();
        var repositoryResult = Result<List<Location>>.Failure("Database error", HttpStatusCode.InternalServerError);

        _mockLocationRepository.Setup(r => r.GetAllAsync(filterOptions))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _locationService.GetAllLocations(filterOptions);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Message.Should().Be("Database error");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetLocationById_WhenLocationExists_ShouldReturnLocation()
    {
        // Arrange
        const int locationId = 1;
        var location = new Location { LocationId = locationId, Name = "Office A", Address = "123 Main St" };
        var repositoryResult = Result<Location>.Success(location, "Location found");

        _mockLocationRepository.Setup(r => r.GetByIdAsync(locationId))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _locationService.GetLocationById(locationId);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.OK);
        result.Data!.LocationId.Should().Be(locationId);
        result.Data.Name.Should().Be("Office A");
        result.Data.Address.Should().Be("123 Main St");
    }

    [Fact]
    public async Task GetLocationById_WhenLocationNotFound_ShouldReturnFailureResponse()
    {
        // Arrange
        const int locationId = 999;
        var repositoryResult = Result<Location>.Failure("Location not found", HttpStatusCode.NotFound);

        _mockLocationRepository.Setup(r => r.GetByIdAsync(locationId))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _locationService.GetLocationById(locationId);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Location not found");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateLocation_WhenSuccessful_ShouldReturnCreatedLocation()
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

        var repositoryResult = Result<Location>.Success(createdLocation, "Location created");
        _mockLocationRepository.Setup(r => r.CreateAsync(locationDto))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _locationService.CreateLocation(locationDto);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.Created);
        result.Data!.LocationId.Should().Be(1);
        result.Data.Name.Should().Be("New Office");
        result.Data.Address.Should().Be("789 Pine St");
    }

    [Fact]
    public async Task CreateLocation_WhenRepositoryFails_ShouldReturnFailureResponse()
    {
        // Arrange
        var locationDto = new LocationApiRequestDto
        {
            Name = "New Office",
            Address = "789 Pine St"
        };

        var repositoryResult = Result<Location>.Failure("Validation error", HttpStatusCode.BadRequest);
        _mockLocationRepository.Setup(r => r.CreateAsync(locationDto))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _locationService.CreateLocation(locationDto);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Be("Validation error");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task UpdateLocation_WhenSuccessful_ShouldReturnUpdatedLocation()
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

        var repositoryResult = Result<Location>.Success(updatedLocation, "Location updated");
        _mockLocationRepository.Setup(r => r.UpdateAsync(locationId, locationDto))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _locationService.UpdateLocation(locationId, locationDto);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.OK);
        result.Data!.LocationId.Should().Be(locationId);
        result.Data.Name.Should().Be("Updated Office");
        result.Data.Address.Should().Be("999 Updated St");
    }

    [Fact]
    public async Task UpdateLocation_WhenRepositoryFails_ShouldReturnFailureResponse()
    {
        // Arrange
        const int locationId = 1;
        var locationDto = new LocationApiRequestDto
        {
            Name = "Updated Office",
            Address = "999 Updated St"
        };

        var repositoryResult = Result<Location>.Failure("Location not found", HttpStatusCode.NotFound);
        _mockLocationRepository.Setup(r => r.UpdateAsync(locationId, locationDto))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _locationService.UpdateLocation(locationId, locationDto);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Location not found");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task DeleteLocation_WhenSuccessful_ShouldReturnSuccessResponse()
    {
        // Arrange
        const int locationId = 1;
        var repositoryResult = Result.Success("Location deleted");

        _mockLocationRepository.Setup(r => r.DeleteAsync(locationId))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _locationService.DeleteLocation(locationId);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeFalse();
        result.ResponseCode.Should().Be(HttpStatusCode.OK);
        result.Message.Should().Be("Location deleted");
    }

    [Fact]
    public async Task DeleteLocation_WhenRepositoryFails_ShouldReturnFailureResponse()
    {
        // Arrange
        const int locationId = 1;
        var repositoryResult = Result.Failure("Location not found", HttpStatusCode.NotFound);

        _mockLocationRepository.Setup(r => r.DeleteAsync(locationId))
            .ReturnsAsync(repositoryResult);

        // Act
        var result = await _locationService.DeleteLocation(locationId);

        // Assert
        result.Should().NotBeNull();
        result.RequestFailed.Should().BeTrue();
        result.ResponseCode.Should().Be(HttpStatusCode.NotFound);
        result.Message.Should().Be("Location not found");
    }
}
