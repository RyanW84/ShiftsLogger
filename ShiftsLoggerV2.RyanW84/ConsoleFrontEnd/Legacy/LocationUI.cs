using ConsoleFrontEnd.Interfaces;
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
    public LocationFilterOptions GetLocationFilterOptions() => _userInterface.FilterLocationsUi();
    public void DisplayLocations(IEnumerable<Location> locations) => _userInterface.DisplayLocationsTable(locations);
    public int SelectLocation() => _userInterface.GetLocationByIdUi();
}
