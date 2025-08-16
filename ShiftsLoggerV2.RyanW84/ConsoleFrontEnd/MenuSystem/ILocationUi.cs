using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.MenuSystem;

public interface ILocationUi
{
    Location CreateLocation();
    Location UpdateLocation(Location existingLocation);
    LocationFilterOptions FilterLocations();
    void DisplayLocationsTable(IEnumerable<Location> locations);
    int GetLocationById();
    int SelectLocation();
}
