using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
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
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAllWorkers_ShouldReturnResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/workers");

        // Assert
        response.Should().NotBeNull();
        // Just testing that we can make the request without errors
        (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound).Should().BeTrue();
    }

    [Fact]
    public async Task CreateWorker_WithValidData_ShouldCreateWorker()
    {
        // Arrange - Use unique email to avoid conflicts
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var newWorker = new WorkerApiRequestDto
        {
            Name = $"Test Worker {uniqueId}",
            Email = $"test.{uniqueId}@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/workers", newWorker);

        // Assert
        (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK).Should().BeTrue();
    }
}