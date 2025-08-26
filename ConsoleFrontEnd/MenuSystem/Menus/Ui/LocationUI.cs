using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.MenuSystem.Common;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public class LocationUI : ILocationUi
{
    private readonly IConsoleDisplayService _display;
    private readonly UiHelper _uiHelper;
    private readonly ILocationService _locationService;

    public LocationUI(IConsoleDisplayService display, ILogger<LocationUI> logger, ILocationService locationService)
    {
        _display = display;
        _uiHelper = new UiHelper(display, logger);
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
    }

    public Location CreateLocationUi()
    {
        _display.DisplayHeader("Create New Location");
        
        var name = AnsiConsole.Ask<string>("[green]Enter location name:[/]");
        var address = AnsiConsole.Ask<string>("[green]Enter address:[/]");
        var town = AnsiConsole.Ask<string>("[green]Enter town:[/]");
        var county = AnsiConsole.Ask<string>("[green]Enter county:[/]");
        var postCode = AnsiConsole.Ask<string>("[green]Enter post code:[/]");
        var country = AnsiConsole.Ask<string>("[green]Enter country:[/]");
        
        return new Location
        {
            LocationId = 0, // Will be assigned by service
            Name = name,
            Address = address,
            Town = town,
            County = county,
            PostCode = postCode,
            Country = country
        };
    }

    public Location UpdateLocationUi(Location existingLocation)
    {
        _display.DisplayHeader($"Update Location: {existingLocation.Name}");
        
        var name = AnsiConsole.Ask("[green]Enter new location name:[/]", existingLocation.Name);
        var address = AnsiConsole.Ask("[green]Enter address:[/]", existingLocation.Address);
        var town = AnsiConsole.Ask("[green]Enter town:[/]", existingLocation.Town);
        var county = AnsiConsole.Ask("[green]Enter county:[/]", existingLocation.County);
        var postCode = AnsiConsole.Ask("[green]Enter post code:[/]", existingLocation.PostCode);
        var country = AnsiConsole.Ask("[green]Enter country:[/]", existingLocation.Country);
        
        return new Location
        {
            LocationId = existingLocation.Id,
            Name = name,
            Address = address,
            Town = town,
            County = county,
            PostCode = postCode,
            Country = country
        };
    }

    public LocationFilterOptions FilterLocationsUi()
    {
        _display.DisplayHeader("Filter Locations");
        
        var name = _uiHelper.GetOptionalStringInput("Filter by name");
        
        return new LocationFilterOptions
        {
            Name = name
        };
    }

    public void DisplayLocationsTable(IEnumerable<Location> locations)
    {
        _display.DisplayTable(locations, "Locations");
    }

    public async Task<int> GetLocationByIdUi()
    {
        _display.DisplayHeader("Select Location", "blue");

        var response = await _locationService.GetAllLocationsAsync().ConfigureAwait(false);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            _uiHelper.DisplayValidationError(response.Message ?? "No locations available.");
            // Fallback to manual entry
            return AnsiConsole.Ask<int>("[green]Enter location ID:[/]");
        }

        var choices = response.Data
            .Select(l => $"{l.LocationId}: {l.Name} - {l.Town}, {l.Country}")
            .ToArray();

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select Location:")
                .AddChoices(choices)
        );

        return UiHelper.ExtractIdFromChoice(selected);
    }

    public async Task<int> SelectLocation()
    {
        _display.DisplayHeader("Select Location", "blue");

        var response = await _locationService.GetAllLocationsAsync().ConfigureAwait(false);
        if (response.RequestFailed || response.Data == null || !response.Data.Any())
        {
            _uiHelper.DisplayValidationError(response.Message ?? "No locations available.");
            // Fallback to manual entry
            return AnsiConsole.Ask<int>("[green]Select location ID:[/]");
        }

        var choices = response.Data
            .Select(l => $"{l.LocationId}: {l.Name} - {l.Town}, {l.Country}")
            .ToArray();

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select Location:")
                .AddChoices(choices)
        );

        return UiHelper.ExtractIdFromChoice(selected);
    }
}
