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
        StateOrCounty = null,
        ZipOrPostCode = null,
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
}
