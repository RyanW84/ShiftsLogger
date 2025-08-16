using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.MenuSystem;

public class LocationUI : ILocationUi
{
    private readonly UserInterface _userInterface;

    public LocationUI(UserInterface userInterface)
    {
        _userInterface = userInterface;
    }

    public Location CreateLocation() => _userInterface.CreateLocationUi();
    public Location UpdateLocation(Location existingLocation) => _userInterface.UpdateLocationUi(existingLocation);
    public LocationFilterOptions FilterLocations() => _userInterface.FilterLocationsUi();
    public void DisplayLocationsTable(IEnumerable<Location> locations) => _userInterface.DisplayLocationsTable(locations);
    public int GetLocationById() => _userInterface.GetLocationByIdUi();
    public int SelectLocation() => GetLocationById();
}
namespace ConsoleFrontEnd.MenuSystem;

public class LocationUI : ILocationUi
{
    private readonly UserInterface _userInterface;

    public LocationUI(UserInterface userInterface)
    {
        _userInterface = userInterface;
    }

    public Location CreateLocationUi() => _userInterface.CreateLocationUi();
    public Location UpdateLocationUi(Location existingLocation) => _userInterface.UpdateLocationUi(existingLocation);
    public LocationFilterOptions FilterLocationsUi() => _userInterface.FilterLocationsUi();
    public void DisplayLocationsTable(IEnumerable<Location> locations) => _userInterface.DisplayLocationsTable(locations);
    public int GetLocationByIdUi() => _userInterface.GetLocationByIdUi();
    public int SelectLocation() => GetLocationByIdUi(); // Implementation for the SelectLocation method
}
