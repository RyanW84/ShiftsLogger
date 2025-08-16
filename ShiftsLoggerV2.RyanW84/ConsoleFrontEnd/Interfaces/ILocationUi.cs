using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd;

public interface ILocationUi
{
    Location CreateLocationUi();
    Location UpdateLocationUi(Location existingLocation);
    LocationFilterOptions FilterLocationsUi();
    void DisplayLocationsTable(IEnumerable<Location> locations);
    int GetLocationByIdUi();
}
