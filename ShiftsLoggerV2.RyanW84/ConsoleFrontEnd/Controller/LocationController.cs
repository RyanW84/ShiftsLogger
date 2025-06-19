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
        TownOrCity = null,
        StateOrCounty = null,
        ZipOrPostCode = null,
        Country = null,
        Search = null,
        SortBy = "Name", // Default sorting by name
        SortOrder = "asc", // Default sorting order is ascending
    };

    // Helpers
    public async Task<ApiResponseDto<Locations>> CheckLocationExists(int locationId)
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
                    return new ApiResponseDto<Locations>
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
            return new ApiResponseDto<Locations>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                Message = $"Exception occurred: {ex.Message}",
                Data = null,
            };
        }
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
            var locations = await locationService.GetAllLocations(locationFilterOptions);

            if (locations.Data is not null)
            {
                userInterface.DisplayLocationsTable(locations.Data);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]No locations found.[/]");
                userInterface.ContinueAndClearScreen();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
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
            var locationId = userInterface.GetLocationByIdUi();
            var location = await CheckLocationExists(locationId);

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

            var locationId = userInterface.GetLocationByIdUi();

            var existingLocation = await CheckLocationExists(locationId);

            if (existingLocation.Data is null)
            {
                userInterface.DisplayErrorMessage(existingLocation.Message);
                userInterface.ContinueAndClearScreen();
                return;
            }

            var updatedLocation = userInterface.UpdateLocationUi(existingLocation.Data);

            var updatedLocationResponse = await locationService.UpdateLocation(
                locationId,
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

            var locationId = userInterface.GetLocationByIdUi();
            var existingLocation = await CheckLocationExists(locationId);

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
