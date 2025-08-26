using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ShiftsLoggerV2.RyanW84.Tests.Integration;

public class WorkersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WorkersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ShiftsLoggerDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add In-Memory database for testing
                services.AddDbContext<ShiftsLoggerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAllWorkers_ShouldReturnEmptyList_WhenNoWorkersExist()
    {
        // Act
        var response = await _client.GetAsync("/api/workers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Worker>>>();
        content.Should().NotBeNull();
        content!.RequestFailed.Should().BeFalse();
        content.Data.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task CreateWorker_ShouldCreateWorkerSuccessfully()
    {
        // Arrange
        var newWorker = new WorkerApiRequestDto
        {
            Name = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "123-456-7890"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/workers", newWorker);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>();
        content.Should().NotBeNull();
        content!.RequestFailed.Should().BeFalse();
        content.Data!.Name.Should().Be("John Doe");
        content.Data.Email.Should().Be("john@example.com");
        content.Data.PhoneNumber.Should().Be("123-456-7890");
        content.Data.WorkerId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetWorkerById_ShouldReturnWorker_WhenWorkerExists()
    {
        // Arrange - First create a worker
        var newWorker = new WorkerApiRequestDto
        {
            Name = "Jane Smith",
            Email = "jane@example.com",
            PhoneNumber = "987-654-3210"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/workers", newWorker);
        var createContent = await createResponse.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>();
        var workerId = createContent!.Data!.WorkerId;

        // Act
        var response = await _client.GetAsync($"/api/workers/{workerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>();
        content.Should().NotBeNull();
        content!.RequestFailed.Should().BeFalse();
        content.Data!.WorkerId.Should().Be(workerId);
        content.Data.Name.Should().Be("Jane Smith");
        content.Data.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task GetWorkerById_ShouldReturnNotFound_WhenWorkerDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/workers/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>();
        content.Should().NotBeNull();
        content!.RequestFailed.Should().BeTrue();
        content.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateWorker_ShouldReturnBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        var invalidWorker = new WorkerApiRequestDto
        {
            Name = "", // Invalid empty name
            Email = "john@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/workers", invalidWorker);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>();
        content.Should().NotBeNull();
        content!.RequestFailed.Should().BeTrue();
        content.Message.Should().Contain("name");
    }

    [Fact]
    public async Task CreateWorker_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        // Arrange
        var invalidWorker = new WorkerApiRequestDto
        {
            Name = "John Doe",
            Email = "invalid-email-format"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/workers", invalidWorker);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>();
        content.Should().NotBeNull();
        content!.RequestFailed.Should().BeTrue();
        content.Message.Should().Contain("Email");
    }

    [Fact]
    public async Task GetAllWorkers_ShouldReturnAllCreatedWorkers()
    {
        // Arrange - Create multiple workers
        var worker1 = new WorkerApiRequestDto { Name = "Alice Johnson" };
        var worker2 = new WorkerApiRequestDto { Name = "Bob Williams" };

        await _client.PostAsJsonAsync("/api/workers", worker1);
        await _client.PostAsJsonAsync("/api/workers", worker2);

        // Act
        var response = await _client.GetAsync("/api/workers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Worker>>>();
        content.Should().NotBeNull();
        content!.RequestFailed.Should().BeFalse();
        content.Data.Should().HaveCountGreaterOrEqualTo(2);
        content.Data!.Should().Contain(w => w.Name == "Alice Johnson");
        content.Data.Should().Contain(w => w.Name == "Bob Williams");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetWorkerById_ShouldReturnBadRequest_WhenIdIsInvalid(int invalidId)
    {
        // Act
        var response = await _client.GetAsync($"/api/workers/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>();
        content.Should().NotBeNull();
        content!.RequestFailed.Should().BeTrue();
        content.Message.Should().Contain("ID must be greater than 0");
    }
}
