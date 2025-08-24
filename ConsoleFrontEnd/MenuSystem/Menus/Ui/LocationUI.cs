using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public class LocationUI : ILocationUi
{
    private readonly IConsoleDisplayService _display;

    public LocationUI(IConsoleDisplayService display)
    {
        _display = display;
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
        
        var name = AnsiConsole.Ask<string>("[yellow]Filter by name (press Enter to skip):[/]", string.Empty);
        
        return new LocationFilterOptions
        {
            Name = string.IsNullOrWhiteSpace(name) ? null : name
        };
    }

    public void DisplayLocationsTable(IEnumerable<Location> locations)
    {
        _display.DisplayTable(locations, "Locations");
    }

    public int GetLocationByIdUi()
    {
        return AnsiConsole.Ask<int>("[green]Enter location ID:[/]");
    }

    public int SelectLocation()
    {
        return AnsiConsole.Ask<int>("[green]Select location ID:[/]");
    }
}
