using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Spectre.Console;

namespace ConsoleFrontEnd.Controller;

public class LocationController
{
    private readonly MenuSystem.UserInterface userInterface = new();
    private readonly LocationService locationService = new();
    private LocationFilterOptions locationFilterOptions = new()
    {
        LocationId = null,
        Name = null,
        Town = null,
        County = null,
        PostCode = null,
        Country = null,
        Search = null,
        SortBy = "Name", // Default sorting by name
        SortOrder = "asc", // Default sorting order is ascending
    };

    // Helpers
    public async Task<ApiResponseDto<Location>> CheckLocationExists(int locationId)
    {
        try
        {
            var response = await locationService.GetLocationById(locationId);

            while (response.ResponseCode is not System.Net.HttpStatusCode.OK)
            {
                userInterface.DisplayErrorMessage(response.Message);
                Console.WriteLine();
                var exitSelection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Try again or exit?")
                        .AddChoices(new[] { "Try Again", "Exit" })
                );
                if (exitSelection is "Exit")
                {
                    return new ApiResponseDto<Location>
                    {
                        RequestFailed = true,
                        ResponseCode = System.Net.HttpStatusCode.NotFound,
                        Message = "User exited the operation.",
                        Data = null,
                    };
                }
                else if (exitSelection is "Try Again")
                {
                    AnsiConsole.Markup("[Yellow]Please enter a correct ID: [/]");
                    locationId = userInterface.GetLocationByIdUi();
                    response = await locationService.GetLocationById(locationId);
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for CheckLocationExists: {ex}");
            return new ApiResponseDto<Location>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Exception occurred: {ex.Message}",
                Data = null,
            };
        }
    }

    public async Task<ApiResponseDto<int>> SelectLocation(
        LocationFilterOptions? locationFilterOptions = null
    )
    {
        // Use default filter if none provided
        locationFilterOptions ??= new LocationFilterOptions();

        // Fetch locations
        var response = await locationService.GetAllLocations(locationFilterOptions);

        if (response.Data == null || response.Data.Count == 0)
        {
            userInterface.DisplayErrorMessage(response.Message);
            return new ApiResponseDto<int>
            {
                RequestFailed = true,
                ResponseCode = response.ResponseCode,
                Message = "No locations available.",
                Data = 0,
            };
        }

        // Prepare choices for the menu
        var choices = response
            .Data.Select(w => new { w.LocationId, Display = $"{w.Name}" })
            .ToList();

        // Show menu and get selection
        var selectedDisplay = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Select a location[/]")
                .AddChoices(choices.Select(c => c.Display))
        );

        // Find the selected location's ID
        var selected = choices.First(c => c.Display == selectedDisplay);

        return new ApiResponseDto<int>
        {
            RequestFailed = false,
            ResponseCode = response.ResponseCode,
            Message = "Location selected.",
            Data = selected.LocationId,
        };
    }

    // CRUD
    public async Task CreateLocation()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Create Location[/]").RuleStyle("yellow").Centered()
            );
            var location = userInterface.CreateLocationUi();
            var createdLocation = await locationService.CreateLocation(location);
            userInterface.ContinueAndClearScreen();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            userInterface.ContinueAndClearScreen();
        }
    }

    public async Task GetAllLocations()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]View All Locations[/]").RuleStyle("yellow").Centered()
            );

            var filterOptions = userInterface.FilterLocationsUi();

            locationFilterOptions = filterOptions;
            var response = await locationService.GetAllLocations(locationFilterOptions);

            if (response.Data is null)
            {
                AnsiConsole.MarkupLine("[red]No locations found.[/]");
                userInterface.ContinueAndClearScreen();
            }
            else
                userInterface.DisplayLocationsTable(response.Data);
            userInterface.ContinueAndClearScreen();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            userInterface.ContinueAndClearScreen();
        }
    }

    public async Task GetLocationById()
    {
        try
        {
            Console.Clear();

            AnsiConsole.Write(
                new Rule("[bold yellow]View Location by ID[/]").RuleStyle("yellow").Centered()
            );
            ApiResponseDto<int>? locationId = await SelectLocation();
            ApiResponseDto<Location> location = await locationService.GetLocationById(
                locationId.Data
            );

            if (location.Data is not null)
            {
                userInterface.DisplayLocationsTable([location.Data]);
                userInterface.ContinueAndClearScreen();
            }
            else
            {
                userInterface.DisplayErrorMessage(location.Message);
                userInterface.ContinueAndClearScreen();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
        }
    }

    public async Task UpdateLocation()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Update Location[/]").RuleStyle("yellow").Centered()
            );

            ApiResponseDto<int>? locationId = await SelectLocation();
            ApiResponseDto<Location> existingLocation = await locationService.GetLocationById(
                locationId.Data
            );

            var updatedLocation = userInterface.UpdateLocationUi(existingLocation.Data);

            var updatedLocationResponse = await locationService.UpdateLocation(
                locationId.Data,
                updatedLocation
            );
            userInterface.DisplaySuccessMessage($"\n{updatedLocationResponse.Message}");
            userInterface.ContinueAndClearScreen();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Exception: {ex.Message}[/]");
        }
    }

    public async Task DeleteLocation()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Delete Location[/]").RuleStyle("yellow").Centered()
            );

            ApiResponseDto<int>? locationId = await SelectLocation();
            ApiResponseDto<Location> existingLocation = await locationService.GetLocationById(
                locationId.Data
            );

            if (existingLocation.Data is null)
            {
                userInterface.DisplayErrorMessage(existingLocation.Message);
                userInterface.ContinueAndClearScreen();
                return;
            }

            var deletedLocationResponse = await locationService.DeleteLocation(
                existingLocation.Data.LocationId
            );

            if (deletedLocationResponse.RequestFailed)
            {
                userInterface.DisplayErrorMessage(deletedLocationResponse.Message);
            }
            else
            {
                userInterface.DisplaySuccessMessage($"\n{deletedLocationResponse.Message}");
            }

            userInterface.ContinueAndClearScreen();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try Pass failed in Location Controller: Delete Location {ex}");
            userInterface.ContinueAndClearScreen();
        }
    }

    // NEW METHODS FOR SEPARATION OF CONCERNS

    // Method to get location input for creation
    public async Task<Location> GetLocationInputAsync()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Create Location - Input[/]").RuleStyle("yellow").Centered()
            );

            var location = userInterface.CreateLocationUi();
            return location;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get location input: {ex.Message}", ex);
        }
    }

    // Method to create location with provided data (no UI interaction)
    public async Task CreateLocationWithData(Location location)
    {
        try
        {
            var response = await locationService.CreateLocation(location);

            if (response.RequestFailed)
            {
                throw new InvalidOperationException(response.Message);
            }

            // Log success but don't display UI here - that's handled in the menu
            Console.WriteLine($"Location created successfully: {response.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create location: {ex.Message}", ex);
        }
    }

    // Method to make SelectLocation async
    public async Task<ApiResponseDto<int>> SelectLocationAsync(LocationFilterOptions? locationFilterOptions = null)
    {
        return await Task.FromResult(await SelectLocation(locationFilterOptions));
    }

    // Method to get location by ID with provided data (no UI interaction)
    public async Task GetLocationByIdWithData(int locationId)
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]View Location by ID - Results[/]").RuleStyle("yellow").Centered()
            );

            var location = await locationService.GetLocationById(locationId);

            if (location.Data is not null)
            {
                userInterface.DisplayLocationsTable([location.Data]);
            }
            else
            {
                throw new InvalidOperationException(location.Message);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get location by ID: {ex.Message}", ex);
        }
    }

    // Method to get update input for location
    public async Task<(int locationId, Location updatedLocation)> GetLocationUpdateInputAsync()
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Update Location - Input[/]").RuleStyle("yellow").Centered()
            );

            // First, select the location to update
            var locationIdResponse = await SelectLocation();
            if (locationIdResponse.RequestFailed)
            {
                throw new InvalidOperationException(locationIdResponse.Message);
            }

            // Get the existing location data
            var existingLocation = await locationService.GetLocationById(locationIdResponse.Data);
            if (existingLocation.Data is null)
            {
                throw new InvalidOperationException(existingLocation.Message);
            }

            // Get the updated location data from user
            var updatedLocation = userInterface.UpdateLocationUi(existingLocation.Data);

            return (locationIdResponse.Data, updatedLocation);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get location update input: {ex.Message}", ex);
        }
    }

    // Method to update location with provided data (no UI interaction)
    public async Task UpdateLocationWithData(int locationId, Location updatedLocation)
    {
        try
        {
            var response = await locationService.UpdateLocation(locationId, updatedLocation);

            if (response.RequestFailed)
            {
                throw new InvalidOperationException(response.Message);
            }

            // Log success but don't display UI here - that's handled in the menu
            Console.WriteLine($"Location updated successfully: {response.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update location: {ex.Message}", ex);
        }
    }

    // Method to delete location with provided data (no UI interaction)
    public async Task DeleteLocationWithData(int locationId)
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Delete Location - Processing[/]").RuleStyle("yellow").Centered()
            );

            // Get location details first to verify it exists
            var existingLocation = await locationService.GetLocationById(locationId);
            if (existingLocation.Data is null)
            {
                throw new InvalidOperationException(existingLocation.Message);
            }

            var response = await locationService.DeleteLocation(existingLocation.Data.LocationId);

            if (response.RequestFailed)
            {
                throw new InvalidOperationException(response.Message);
            }

            // Log success but don't display UI here - that's handled in the menu
            Console.WriteLine($"Location deleted successfully: {response.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete location: {ex.Message}", ex);
        }
    }

    public async Task<LocationFilterOptions> GetLocationFilterInputAsync()
    {
        return await Task.FromResult(userInterface.FilterLocationsUi());
    }

    public async Task GetAllLocationsWithData(LocationFilterOptions filterOptions)
    {
        try
        {
            Console.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]View All Locations - Results[/]").RuleStyle("yellow").Centered()
            );

            var response = await locationService.GetAllLocations(filterOptions);

            if (response.Data is null || response.Data.Count == 0)
            {
                throw new InvalidOperationException("No locations found.");
            }
            else
            {
                userInterface.DisplayLocationsTable(response.Data);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get all locations: {ex.Message}", ex);
        }
    }
}
