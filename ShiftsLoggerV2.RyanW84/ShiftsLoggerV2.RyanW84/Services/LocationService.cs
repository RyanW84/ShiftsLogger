using System.Net;
using Microsoft.EntityFrameworkCore;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using Spectre.Console;

namespace ShiftsLoggerV2.RyanW84.Services;

public class LocationService(ShiftsLoggerDbContext dbContext) : ILocationService
{
    public async Task<ApiResponseDto<List<Location>>> GetAllLocations(
        LocationFilterOptions locationOptions
    )
    {
        var query = dbContext.Locations.AsQueryable<Location>();

        // Apply all filters
        if (locationOptions.LocationId != null && locationOptions.LocationId is not 0)
            query = query.Where(l => l.LocationId == locationOptions.LocationId);

        if (!string.IsNullOrWhiteSpace(locationOptions.Name))
            query = query.Where(l => EF.Functions.Like(l.Name, $"%{locationOptions.Name}%"));
        if (!string.IsNullOrWhiteSpace(locationOptions.Address))
            query = query.Where(l => EF.Functions.Like(l.Address, $"%{locationOptions.Address}%"));
        if (!string.IsNullOrWhiteSpace(locationOptions.Town))
            query = query.Where(l =>
                EF.Functions.Like(l.Town, $"%{locationOptions.Town}%")
            );
        if (!string.IsNullOrWhiteSpace(locationOptions.County))
            query = query.Where(l =>
                EF.Functions.Like(l.County, $"%{locationOptions.County}%")
            );
        if (!string.IsNullOrWhiteSpace(locationOptions.State))
            query = query.Where(l =>
                EF.Functions.Like(l.County, $"%{locationOptions.State}%")
            );
        if (!string.IsNullOrWhiteSpace(locationOptions.PostCode))
            query = query.Where(l =>
                EF.Functions.Like(l.PostCode, $"%{locationOptions.PostCode}%")
            );
        if (!string.IsNullOrWhiteSpace(locationOptions.Country))
            query = query.Where(l => EF.Functions.Like(l.Country, $"%{locationOptions.Country}%"));

        // Simplified search implementation
        if (!string.IsNullOrWhiteSpace(locationOptions.Search))
            query = query.Where(l =>
                l.LocationId.ToString().Contains(locationOptions.Search)
                || EF.Functions.Like(l.Name, $"%{locationOptions.Search}%")
                || EF.Functions.Like(l.Address, $"%{locationOptions.Search}%")
                || EF.Functions.Like(l.Town, $"%{locationOptions.Search}%")
                || EF.Functions.Like(l.Country, $"%{locationOptions.Search}%")
                || EF.Functions.Like(l.PostCode, $"%{locationOptions.Search}%")
                || EF.Functions.Like(l.Country, $"%{locationOptions.Search}%")
            );

        if (!string.IsNullOrWhiteSpace(locationOptions.SortBy))
        {
            locationOptions.SortBy = locationOptions.SortBy.ToLowerInvariant();
            locationOptions.SortOrder =
                locationOptions.SortOrder?.ToLowerInvariant() ?? "asc"; // Normalize sort order to lowercase with default
        }
        else
        {
            locationOptions.SortBy = "locationid"; // Default sort by LocationId if not specified
        }

        AnsiConsole.MarkupLine(
            $"[yellow]Applying sorting:[/] SortBy='{locationOptions.SortBy}', SortOrder='{locationOptions.SortOrder}'"
        );

        // Always apply sorting - whether SortBy is specified or not
        query = locationOptions.SortBy switch
        {
            "locationid" => locationOptions.SortOrder == "asc"
                ? query.OrderBy(l => l.LocationId)
                : query.OrderByDescending(l => l.LocationId),
            "name" => locationOptions.SortOrder == "asc"
                ? query.OrderBy(l => l.Name)
                : query.OrderByDescending(l => l.Name),
            "address" => locationOptions.SortOrder == "asc"
                ? query.OrderBy(l => l.Address)
                : query.OrderByDescending(l => l.Address),
            "townorcity" => locationOptions.SortOrder == "asc"
                ? query.OrderBy(l => l.Name)
                : query.OrderByDescending(l => l.Town),
            "stateorcounty" => locationOptions.SortOrder == "asc"
                ? query.OrderBy(l => l.Name)
                : query.OrderByDescending(l => l.Country),
            "ziporpostcode" => locationOptions.SortOrder == "asc"
                ? query.OrderBy(l => l.Name)
                : query.OrderByDescending(l => l.PostCode),
            "country" => locationOptions.SortOrder == "asc"
                ? query.OrderBy(l => l.Name)
                : query.OrderByDescending(l => l.Country),
            _ => query.OrderBy(l => l.LocationId) // Default sorting by LocationId
        };

        // Execute query and get results
        List<Location> locations = [.. (await query.ToListAsync()).Cast<Location>()];

        if (locations.Count == 0)
            return new ApiResponseDto<List<Location>>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.NotFound,
                Message = "No locations found with the specified criteria.",
                Data = locations
            };

        return new ApiResponseDto<List<Location>>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = "Locations retrieved successfully.",
            Data = locations
        };
    }

    public async Task<ApiResponseDto<Location>> GetLocationById(int id)
    {
        var location = await dbContext.Locations.FirstOrDefaultAsync(w =>
            w.LocationId == id
        );

        if (location is null)
            return new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.NotFound,
                Message = "Location not found.",
                Data = null
            };

        return new ApiResponseDto<Location>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = "Location retrieved succesfully",
            Data = location
        };
    }

    public async Task<ApiResponseDto<Location>> CreateLocation(LocationApiRequestDto location)
    {
        try
        {
            Location newLocation = new()
            {
                Name = location.Name,
                Address = location.Address,
                Town = location.Town,
                County = location.State,
                PostCode = location.PostCode,
                Country = location.Country
            };
            var savedLocation = await dbContext.Locations.AddAsync(newLocation);
            await dbContext.SaveChangesAsync();

            return new ApiResponseDto<Location>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.Created,
                Message = "Location created successfully.",
                Data = savedLocation.Entity
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Back end location service - {ex}");
            return new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                Message = "An error occurred while creating the location.",
                Data = null
            };
        }
    }

    public async Task<ApiResponseDto<Location>> UpdateLocation(
        int id,
        LocationApiRequestDto updatedLocation
    )
    {
        var savedLocation = await dbContext.Locations.FindAsync(id);

        if (savedLocation is null)
            return new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.NotFound,
                Message = "Location not found",
                Data = null!
            };
        savedLocation.LocationId = id; // Ensure the LocationId is set to the ID being updated
        savedLocation.Name = updatedLocation.Name;
        savedLocation.Address = updatedLocation.Address;
        savedLocation.Town = updatedLocation.Town;
        savedLocation.Country = updatedLocation.State;
        savedLocation.PostCode = updatedLocation.PostCode;
        savedLocation.Country = updatedLocation.Country;

        dbContext.Locations.Update(savedLocation);
        await dbContext.SaveChangesAsync();

        return new ApiResponseDto<Location>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = "Location updated succesfully",
            Data = savedLocation
        };
    }

    public async Task<ApiResponseDto<string?>> DeleteLocation(int id)
    {
        var savedLocation = await dbContext.Locations.FindAsync(id);

        if (savedLocation is null)
            return new ApiResponseDto<string?>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.NotFound,
                Message = $"Location with ID: {id} not found.",
                Data = null
            };

        dbContext.Locations.Remove(savedLocation);
        await dbContext.SaveChangesAsync();

        return new ApiResponseDto<string?>
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            Message = $"Location with ID: {id} deleted successfully.",
            Data = null
        };
    }
}