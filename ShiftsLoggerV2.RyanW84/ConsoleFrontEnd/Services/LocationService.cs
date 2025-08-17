using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services;

public class LocationService : ILocationService
{
    public Task<ApiResponseDto<List<Location>>> GetAllLocationsAsync()
    {
        // Mock implementation using correct property names
        var locations = new List<Location>
        {
            new Location 
            { 
                LocationId = 1, 
                Name = "Main Office", 
                Address = "123 Main St",
                Town = "London",
                County = "Greater London",
                PostCode = "SW1A 1AA",
                Country = "United Kingdom"
            },
            new Location 
            { 
                LocationId = 2, 
                Name = "Branch Office",
                Address = "456 High St",
                Town = "Manchester",
                County = "Greater Manchester",
                PostCode = "M1 1AA",
                Country = "United Kingdom"
            }
        };

        return Task.FromResult(new ApiResponseDto<List<Location>>("Success")
        {
            Data = locations,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        });
    }

    public Task<ApiResponseDto<Location?>> GetLocationByIdAsync(int id)
    {
        // Mock implementation using correct property names
        var location = new Location 
        { 
            LocationId = id, 
            Name = $"Location {id}",
            Address = $"{id} Example St",
            Town = "London",
            County = "Greater London",
            PostCode = "SW1A 1AA",
            Country = "United Kingdom"
        };

        return Task.FromResult(new ApiResponseDto<Location?>("Success")
        {
            Data = location,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        });
    }

    public Task<ApiResponseDto<Location>> CreateLocationAsync(Location location)
    {
        // Mock implementation - assign an ID
        location.LocationId = new Random().Next(1, 1000);

        return Task.FromResult(new ApiResponseDto<Location>("Location created successfully")
        {
            Data = location,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.Created
        });
    }

    public Task<ApiResponseDto<Location?>> UpdateLocationAsync(int id, Location updatedLocation)
    {
        // Mock implementation
        updatedLocation.LocationId = id;

        return Task.FromResult(new ApiResponseDto<Location?>("Location updated successfully")
        {
            Data = updatedLocation,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        });
    }

    public Task<ApiResponseDto<string?>> DeleteLocationAsync(int id)
    {
        // Mock implementation
        return Task.FromResult(new ApiResponseDto<string?>("Location deleted successfully")
        {
            Data = $"Deleted location with ID {id}",
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        });
    }
}
