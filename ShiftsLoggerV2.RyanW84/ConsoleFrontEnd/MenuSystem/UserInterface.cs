using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public class UserInterface
{
    private readonly ShiftService shiftService;

    // UI method: Handles user interaction
    // and displays the results of the operations

    // Helpers
    public void ContinueAndClearScreen()
    {
        {
            Console.WriteLine("\nPress any key to continue");
            Console.ReadKey();
            Console.Clear();
        }
    }

    public void DisplayErrorMessage(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }

    public void DisplaySuccessMessage(string message)
    {
        AnsiConsole.MarkupLine($"[green]{message}[/]");
    }

    // Constructor
    public UserInterface()
    {
        shiftService = new ShiftService();
    }

    // Shifts
    public ShiftFilterOptions FilterShiftsUi()
    {
        var filterOptions = new ShiftFilterOptions
        {
            ShiftId = null,
            WorkerId = null,
            LocationId = null,
            StartTime = null,
            EndTime = null,
            Search = null,
            LocationName = null,
            StartDate = null,
            EndDate = null,
            SortBy = null,
            SortOrder = null,
        };
        // 1. Gather user input (UI Layer)
        AnsiConsole.WriteLine("\nPlease enter filter criteria (leave blank to skip):");
        var filterCriteria = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Dp you wish to apply any Filters?:[/]")
                .AddChoices("Yes", "No")
        );

        if (filterCriteria == "No")
        {
            AnsiConsole.MarkupLine("[green]No filters applied.[/]");
            return filterOptions; // Return default filter options with null values
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Choose which filters...[/]");
            filterOptions.ShiftId = AnsiConsole.Ask<int?>(
                "Enter [green]Shift #[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.WorkerId = AnsiConsole.Ask<int?>(
                "Enter [green]Worker #[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.LocationId = AnsiConsole.Ask<int?>(
                "Enter [green]Location #[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.StartDate = AnsiConsole.Ask<DateTime?>(
                "Enter [green]Start Date[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.EndDate = AnsiConsole.Ask<DateTime?>(
                "Enter [green]End Date[/] (or leave blank):",
                defaultValue: null
            );

            filterOptions.StartTime = AnsiConsole.Ask<DateTime?>(
                "Enter [green]Start Time[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.EndTime = AnsiConsole.Ask<DateTime?>(
                "Enter [green]End Time[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.Search = AnsiConsole.Ask<string?>(
                "Enter [green]Search criteria[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.LocationName = AnsiConsole.Ask<string?>(
                "Enter [green]Location Name[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.SortBy = AnsiConsole.Ask<string?>(
                "Enter [green]Sort By[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.SortOrder = AnsiConsole.Ask<string?>(
                "Enter [green]Sort Order[/] (or leave blank):",
                defaultValue: null
            );

            return filterOptions;
        }
    }

    public Shift CreateShiftUi(int workerId)
    {
        // 1. Gather user input (UI Layer)
        AnsiConsole.WriteLine("\nPlease enter the following details for the shift:");
		
		var startTime = AnsiConsole.Ask<DateTime>("Enter [green]Start Time[/]:");
        var endTime = AnsiConsole.Ask<DateTime>("Enter [green]End Time[/]:");
      
      

        var createdShift = new Shift
        {
            StartTime = startTime,
            EndTime = endTime,
            LocationId = locationId,
            WorkerId = workerId,
        };

        return createdShift;
    }

    public void DisplayShiftsTable(IEnumerable<Shift> shiftsResponse)
    {
        if (shiftsResponse is null)
        {
            AnsiConsole.MarkupLine("[red]No shifts found.[/]");
            ContinueAndClearScreen();
            return;
        }
        Table table = new();
        table.AddColumn("Index #");
        table.AddColumn("Worker #");
        table.AddColumn("Location #");
        table.AddColumn("Start Time");
        table.AddColumn("End Time");
        table.AddColumn("Duration");
        List<Shift> shiftList = shiftsResponse.ToList();

        for (int i = 0; i < shiftList.Count; i++)
        {
            var shift = shiftList[i];
            if (shift != null)
            {
                table.AddRow(
                    (i + 1).ToString(),
                    shift.WorkerId.ToString(),
                    shift.LocationId.ToString(),
                    shift.StartTime.ToString("g"), // Format DateTimeOffset to a readable string
                    shift.EndTime.ToString("g"), // Format DateTimeOffset to a readable string
                    (shift.EndTime - shift.StartTime).ToString(@"hh\:mm") // Calculate duration and format as hours and minutes
                );
            }
        }
        AnsiConsole.Write(table);
    }

    public int GetShiftByIdUi()
    {
        // 1. Gather user input (UI Layer)
        Console.WriteLine();
        var shiftId = AnsiConsole.Ask<int>($"Enter [green]Shift ID:[/] ");

        return shiftId;
    }

    public Shift UpdateShiftUi(Shift existingShift)
    {
        var startTime = AnsiConsole.Ask<DateTime?>(
            "Enter [green]Start Time[/] (leave blank to keep current):",
            existingShift.StartTime.DateTime
        );
        var endTime = AnsiConsole.Ask<DateTime?>(
            "Enter [green]End Time[/] (leave blank to keep current):",
            existingShift.EndTime.DateTime
        );
        var locationId = AnsiConsole.Ask<int?>(
            "Enter [green]Location ID[/] (leave blank to keep current):",
            existingShift.LocationId
        );
        var workerId = AnsiConsole.Ask<int?>(
            "Enter [green]Worker ID[/] (leave blank to keep current):",
            existingShift.WorkerId
        );

        var updatedShift = new Shift
        {
            StartTime = startTime ?? existingShift.StartTime,
            EndTime = endTime ?? existingShift.EndTime,
            LocationId = locationId ?? existingShift.LocationId,
            WorkerId = workerId ?? existingShift.WorkerId,
        };

        return updatedShift;
    }

    // Workers
    public WorkerFilterOptions FilterWorkersUi()
    {
        var filterOptions = new WorkerFilterOptions
        {
            WorkerId = null,
            Name = null,
            PhoneNumber = null,
            Email = null,
            Search = null,
            SortBy = null,
            SortOrder = null,
        };

        // Gather user input (UI Layer)
        AnsiConsole.WriteLine("\nPlease enter filter criteria for Workers (leave blank to skip):");
        var filterCriteria = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Do you wish to apply any Filters?:[/]")
                .AddChoices("Yes", "No")
        );

        if (filterCriteria == "No")
        {
            AnsiConsole.MarkupLine("[green]No filters applied.[/]");
            return filterOptions; // Return default filter options with null values
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Choose which filters...[/]");
            filterOptions.WorkerId = AnsiConsole.Ask<int?>(
                "Enter [green]Worker ID[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.Name = AnsiConsole.Ask<string?>(
                "Enter [green]Name[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.PhoneNumber = AnsiConsole.Ask<string?>(
                "Enter [green]Phone Number[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.Email = AnsiConsole.Ask<string?>(
                "Enter [green]Email[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.Search = AnsiConsole.Ask<string?>(
                "Enter [green]Search criteria[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.SortBy = AnsiConsole.Ask<string?>(
                "Enter [green]Sort By[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.SortOrder = AnsiConsole.Ask<string?>(
                "Enter [green]Sort Order[/] (ASC or DESC...or leave blank):",
                defaultValue: null
            );

            return filterOptions; // Return the filter options with user input
        }
    }

    public Worker CreateWorkerUi()
    {
        // 1. Gather user input (UI Layer)
        AnsiConsole.WriteLine("\nPlease enter the following details for the worker:");
        var name = AnsiConsole.Ask<string>("Enter [green]Name[/]:");
        var email = AnsiConsole.Ask<string>("Enter [green]Email[/]:");
        var phoneNumber = AnsiConsole.Ask<string>("Enter [green]Phone Number[/]:");
        var createdWorker = new Worker
        {
            Name = name,
            Email = email,
            PhoneNumber = phoneNumber,
        };

        return createdWorker;
    }

    public void DisplayWorkersTable(IEnumerable<Worker> workers)
    {
        Table table = new();
        table.AddColumn("Index #");
        table.AddColumn("Name");
        table.AddColumn("Email");
        table.AddColumn("Phone Number");

        var workersList = workers.ToList();
        for (int i = 0; i < workersList.Count; i++)
        {
            var worker = workersList[i];
            if (worker != null)
            {
                table.AddRow(
                    (i + 1).ToString(),
                    worker.Name,
                    worker.Email ?? "N/A", // Handle null email addresses
                    worker.PhoneNumber ?? "N/A" // Handle null phone numbers
                );
            }
        }
        AnsiConsole.Write(table);
    }

    public int GetWorkerByIdUi()
    {
        // 1. Gather user input (UI Layer)
        var workerId = AnsiConsole.Ask<int>($"Enter [green]Worker ID:[/] ");
        return workerId;
    }

    public Worker UpdateWorkerUi(Worker existingWorker)
    {
        var name = AnsiConsole.Ask<string>(
            "Enter [green]Name[/] (leave blank to keep current):",
            existingWorker.Name
        );
        var email = AnsiConsole.Ask<string>(
            "Enter [green]Email[/] (leave blank to keep current):",
            existingWorker.Email
        );
        var phoneNumber = AnsiConsole.Ask<string>(
            "Enter [green]Phone Number[/] (leave blank to keep current):",
            existingWorker.PhoneNumber
        );
        var updatedWorker = new Worker
        {
            Name = name,
            Email = email,
            PhoneNumber = phoneNumber,
        };
        return updatedWorker;
    }

    // Locations
    public LocationFilterOptions FilterLocationsUi()
    {
        var filterLocationOptions = new LocationFilterOptions
        {
            LocationId = null,
            Name = null,
            Address = null,
            TownOrCity = null,
            StateOrCounty = null,
            ZipOrPostCode = null,
            Country = null,
            Search = null,
            SortBy = null,
            SortOrder = null,
        };
        // 1. Gather user input (UI Layer)
        AnsiConsole.WriteLine(
            "\nPlease enter filter criteria for Locations (leave blank to skip):"
        );
        var filterCriteria = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Do you wish to apply any Filters?:[/]")
                .AddChoices("Yes", "No")
        );
        if (filterCriteria == "No")
        {
            AnsiConsole.MarkupLine("[green]No filters applied.[/]");
            return filterLocationOptions; // Return default filter options with null values
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Choose which filters...[/]");
            filterLocationOptions.LocationId = AnsiConsole.Ask<int?>(
                "Enter [green]Location ID[/] (or leave blank):",
                defaultValue: null
            );
            filterLocationOptions.Name = AnsiConsole.Ask<string?>(
                "Enter [green]Name[/] (or leave blank):",
                defaultValue: null
            );
            filterLocationOptions.Address = AnsiConsole.Ask<string?>(
                "Enter [green]Address[/] (or leave blank):",
                defaultValue: null
            );
            filterLocationOptions.TownOrCity = AnsiConsole.Ask<string?>(
                "Enter [green]Town or City[/] (or leave blank):",
                defaultValue: null
            );
            filterLocationOptions.StateOrCounty = AnsiConsole.Ask<string?>(
                "Enter [green]State or County[/] (or leave blank):",
                defaultValue: null
            );
            filterLocationOptions.ZipOrPostCode = AnsiConsole.Ask<string?>(
                "Enter [green]Zip or Post Code[/] (or leave blank):",
                defaultValue: null
            );
            filterLocationOptions.Country = AnsiConsole.Ask<string?>(
                "Enter [green]Country[/] (or leave blank):",
                defaultValue: null
            );
            filterLocationOptions.Search = AnsiConsole.Ask<string?>(
                "Enter [green]Search Criteria[/] (or leave blank):",
                defaultValue: null
            );
            filterLocationOptions.SortBy = AnsiConsole.Ask<string?>(
                "Enter [green]Sort By[/] (or leave blank):",
                defaultValue: null
            );
            filterLocationOptions.SortOrder = AnsiConsole.Ask<string?>(
                "Enter [green]Sort Order[/] (or leave blank):",
                defaultValue: null
            );

            return filterLocationOptions; // Return the filter options with user input
        }
    }

    public Location CreateLocationUi()
    {
        // 1. Gather user input (UI Layer)
        AnsiConsole.WriteLine("\nPlease enter the following details for the worker:");
        var name = AnsiConsole.Ask<string>("Enter [green]Name[/]:");
        var address = AnsiConsole.Ask<string>("Enter [green]Address[/]:");
        var townOrCity = AnsiConsole.Ask<string>("Enter [green]Town or City[/]:");
        var stateOrCounty = AnsiConsole.Ask<string>("Enter [green]State or County[/]:");
        var zipOrPostCode = AnsiConsole.Ask<string>("Enter [green]Zip or Post Code[/]:");
        var country = AnsiConsole.Ask<string>("Enter [green]Country[/]:");
        var createdLocation = new Location
        {
            Name = name,
            Address = address,
            TownOrCity = townOrCity,
            StateOrCounty = stateOrCounty,
            ZipOrPostCode = zipOrPostCode,
            Country = country,
        };

        return createdLocation;
    }

    public void DisplayLocationsTable(IEnumerable<Location> locationResponse)
    {
        if (locationResponse is null)
        {
            AnsiConsole.MarkupLine("[red]No locations found.[/]");
            ContinueAndClearScreen();
            return;
        }
        Table table = new();
        table.AddColumn("Index #");
        table.AddColumn("Name");
        table.AddColumn("Address");
        table.AddColumn("Town or City");
        table.AddColumn("State or County");
        table.AddColumn("Zip or Post Code");
        table.AddColumn("Country");
        // Convert IEnumerable to List for easier indexing
        List<Location> locationsList = locationResponse.ToList();

        for (int i = 0; i < locationsList.Count; i++)
        {
            var location = locationsList[i];
            if (location != null)
            {
                table.AddRow(
                    (i + 1).ToString(),
                    location.Name,
                    location.Address,
                    location.TownOrCity,
                    location.StateOrCounty,
                    location.ZipOrPostCode,
                    location.Country
                );
            }
        }
        AnsiConsole.Write(table);
    }

    public int GetLocationByIdUi()
    {
        // 1. Gather user input (UI Layer)
        var locationId = AnsiConsole.Ask<int>($"Enter [green]Location ID:[/] ");
        return locationId;
    }

    public Location UpdateLocationUi(Location existingLocation)
    {
        var name = AnsiConsole.Ask<string>(
            "Enter [green]Name[/] (leave blank to keep current):",
            existingLocation.Name ?? string.Empty
        );
        var address = AnsiConsole.Ask<string>(
            "Enter [green]Email[/] (leave blank to keep current):",
            existingLocation.Address ?? string.Empty
        );
        var townOrCity = AnsiConsole.Ask<string>(
            "Enter [green]Phone Number[/] (leave blank to keep current):",
            existingLocation.TownOrCity ?? string.Empty
        );

        var stateOrCounty = AnsiConsole.Ask<string>(
            "Enter [green]State or County[/] (leave blank to keep current):",
            existingLocation.StateOrCounty ?? string.Empty
        );

        var zipOrPostCode = AnsiConsole.Ask<string>(
            "Enter [green]Zip or Post Code[/] (leave blank to keep current):",
            existingLocation.ZipOrPostCode ?? string.Empty
        );

        var country = AnsiConsole.Ask<string>(
            "Enter [green]Country[/] (leave blank to keep current):",
            existingLocation.Country ?? string.Empty
        );
        var updatedLocation = new Location
        {
            Name = name,
            Address = address,
            TownOrCity = townOrCity,
            StateOrCounty = stateOrCounty,
            ZipOrPostCode = zipOrPostCode,
            Country = country,
        };
        return updatedLocation;
    }
}
