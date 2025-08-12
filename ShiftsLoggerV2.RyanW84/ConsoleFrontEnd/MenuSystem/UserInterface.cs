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
    private readonly LocationService locationService;

    // Existing helper methods...
    public void ContinueAndClearScreen()
    {
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
        Console.Clear();
    }

    public void DisplayErrorMessage(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }

    public void DisplaySuccessMessage(string message)
    {
        AnsiConsole.MarkupLine($"[green]{message}[/]");
    }

    public UserInterface()
    {
        shiftService = new ShiftService();
        locationService = new LocationService();
    }

    // SHIFT-SPECIFIC UI BUSINESS LOGIC
    /// <summary>
    /// Gets validated shift start and end times with business rules
    /// </summary>
    private (DateTime startTime, DateTime endTime) GetShiftDateTimeRange(string contextMessage = "shift")
    {
        AnsiConsole.MarkupLine($"[yellow]Enter {contextMessage} start and end times:[/]");
        AnsiConsole.MarkupLine("[dim]Supported format: dd/MM/yyyy HH:mm (e.g., 25/12/2024 13:24)[/]");
        AnsiConsole.WriteLine();

        // Get start time first
        var startTime = InputValidator.GetFlexibleDateTime("Enter Start Time:");
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]Start time set: {startTime:dd/MM/yyyy HH:mm}[/]");
        AnsiConsole.MarkupLine("[dim]End time must be after start time...[/]");
        AnsiConsole.WriteLine();

        // Get end time with validation that it's after start time
        var endTime = InputValidator.GetFlexibleDateTime("Enter End Time:", startTime.AddMinutes(1));

        // Show summary
        AnsiConsole.WriteLine();
        var duration = endTime - startTime;
        AnsiConsole.MarkupLine($"[green]✓ Start: {startTime:dd/MM/yyyy HH:mm}[/]");
        AnsiConsole.MarkupLine($"[green]✓ End: {endTime:dd/MM/yyyy HH:mm}[/]");
        AnsiConsole.MarkupLine($"[green]✓ Duration: {duration.Hours:D2}:{duration.Minutes:D2}[/]");
        AnsiConsole.WriteLine();

        return (startTime, endTime);
    }

    /// <summary>
    /// Gets validated shift start and end times for updates with existing values
    /// </summary>
    private (DateTime startTime, DateTime endTime) GetShiftDateTimeRangeForUpdate(DateTime existingStart, DateTime existingEnd)
    {
        AnsiConsole.MarkupLine("[yellow]Update shift times (leave blank to keep current):[/]");
        AnsiConsole.MarkupLine($"[dim]Current start: {existingStart:dd/MM/yyyy HH:mm}[/]");
        AnsiConsole.MarkupLine($"[dim]Current end: {existingEnd:dd/MM/yyyy HH:mm}[/]");
        AnsiConsole.MarkupLine($"[dim]Current duration: {(existingEnd - existingStart).Hours:D2}:{(existingEnd - existingStart).Minutes:D2}[/]");
        AnsiConsole.WriteLine();

        // Ask for new start time (optional)
        var startTimeInput = AnsiConsole.Ask<string>("[green]Enter Start Time[/] [dim](dd/MM/yyyy HH:mm or leave blank):[/]");
        DateTime startTime = existingStart;
        
        if (!string.IsNullOrWhiteSpace(startTimeInput))
        {
            // Use a simple validation loop instead of InputValidator for optional input
            while (true)
            {
                if (DateTime.TryParseExact(startTimeInput, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsed))
                {
                    startTime = parsed;
                    break;
                }
                AnsiConsole.MarkupLine("[red]Invalid format. Please use dd/MM/yyyy HH:mm or leave blank.[/]");
                startTimeInput = AnsiConsole.Ask<string>("[green]Enter Start Time[/] [dim](dd/MM/yyyy HH:mm or leave blank):[/]");
                if (string.IsNullOrWhiteSpace(startTimeInput))
                {
                    startTime = existingStart;
                    break;
                }
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]Start time: {startTime:dd/MM/yyyy HH:mm}[/]");
        
        // Ask for new end time (optional, but must be after start time)
        var endTimeInput = AnsiConsole.Ask<string>("[green]Enter End Time[/] [dim](dd/MM/yyyy HH:mm or leave blank):[/]");
        DateTime endTime = existingEnd;

        if (!string.IsNullOrWhiteSpace(endTimeInput))
        {
            while (true)
            {
                if (DateTime.TryParseExact(endTimeInput, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsed))
                {
                    if (parsed > startTime)
                    {
                        endTime = parsed;
                        break;
                    }
                    AnsiConsole.MarkupLine($"[red]End time must be after start time ({startTime:dd/MM/yyyy HH:mm}).[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid format. Please use dd/MM/yyyy HH:mm or leave blank.[/]");
                }
                endTimeInput = AnsiConsole.Ask<string>("[green]Enter End Time[/] [dim](dd/MM/yyyy HH:mm or leave blank):[/]");
                if (string.IsNullOrWhiteSpace(endTimeInput))
                {
                    endTime = existingEnd;
                    break;
                }
            }
        }

        // Ensure end time is still after start time (in case user only changed start time)
        if (endTime <= startTime)
        {
            AnsiConsole.MarkupLine("[yellow]End time needs to be adjusted as it's now before/equal to start time.[/]");
            endTime = InputValidator.GetFlexibleDateTime("Enter new End Time:", startTime.AddMinutes(1));
        }

        // Show final summary
        AnsiConsole.WriteLine();
        var duration = endTime - startTime;
        AnsiConsole.MarkupLine($"[green]✓ Updated Start: {startTime:dd/MM/yyyy HH:mm}[/]");
        AnsiConsole.MarkupLine($"[green]✓ Updated End: {endTime:dd/MM/yyyy HH:mm}[/]");
        AnsiConsole.MarkupLine($"[green]✓ New Duration: {duration.Hours:D2}:{duration.Minutes:D2}[/]");
        AnsiConsole.WriteLine();

        return (startTime, endTime);
    }

    /// <summary>
    /// Gets a location selection from existing locations
    /// </summary>
    private int GetLocationSelection()
    {
        try
        {
            // Fetch all locations synchronously (following existing pattern)
            var locationResponse = locationService.GetAllLocations(new LocationFilterOptions()).GetAwaiter().GetResult();
            
            if (locationResponse.Data == null || locationResponse.Data.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No locations available. Please create locations first.[/]");
                throw new InvalidOperationException("No locations available for selection.");
            }

            // Create selection choices with location display information
            var locationChoices = locationResponse.Data
                .Select(location => new { 
                    location.LocationId, 
                    Display = $"ID: {location.LocationId} | {location.Name} - {location.Town}, {location.County}" 
                })
                .ToList();

            // Show selection prompt
            var selectedDisplay = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select a location:[/]")
                    .AddChoices(locationChoices.Select(choice => choice.Display))
            );

            // Find the selected location's ID
            var selectedLocation = locationChoices.First(choice => choice.Display == selectedDisplay);
            
            AnsiConsole.MarkupLine($"[green]✓ Selected location: {selectedDisplay}[/]");
            return selectedLocation.LocationId;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error loading locations: {ex.Message}[/]");
            throw;
        }
    }

    public Shift CreateShiftUi(int workerId)
    {
        AnsiConsole.WriteLine("\nPlease enter the following details for the shift:");
        
        // Use the business-specific date range method
        var (startTime, endTime) = GetShiftDateTimeRange("new shift");
        
        // Get location selection from existing locations instead of manual ID input
        var locationId = GetLocationSelection();
        
        var createdShift = new Shift
        {
            StartTime = startTime,
            EndTime = endTime,
            LocationId = locationId,
            WorkerId = workerId,
        };

        return createdShift;
    }

    public Shift UpdateShiftUi(Shift existingShift)
    {
        // Use the business-specific update method
        var (startTime, endTime) = GetShiftDateTimeRangeForUpdate(
            existingShift.StartTime.DateTime,
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
            StartTime = startTime,
            EndTime = endTime,
            LocationId = locationId ?? existingShift.LocationId,
            WorkerId = workerId ?? existingShift.WorkerId,
        };

        return updatedShift;
    }

    // SHIFT FILTER AND DISPLAY METHODS
    public ShiftFilterOptions FilterShiftsUi()
    {
        var filterOptions = new ShiftFilterOptions
        {
            ShiftId = null,
            WorkerId = null,
            StartTime = null,
            EndTime = null,
            StartDate = null,
            EndDate = null,
            LocationName = null,
            Search = null,
            SortBy = null,
            SortOrder = null,
        };

        AnsiConsole.WriteLine("\nPlease enter filter criteria for Shifts (leave blank to skip):");
        var filterCriteria = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Do you wish to apply any Filters?:[/]")
                .AddChoices("Yes", "No")
        );

        if (filterCriteria == "No")
        {
            AnsiConsole.MarkupLine("[green]No filters applied.[/]");
            return filterOptions;
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Choose which filters...[/]");
            filterOptions.ShiftId = AnsiConsole.Ask<int?>(
                "Enter [green]Shift ID[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.WorkerId = AnsiConsole.Ask<int?>(
                "Enter [green]Worker ID[/] (or leave blank):",
                defaultValue: null
            );
            filterOptions.LocationName = AnsiConsole.Ask<string?>(
                "Enter [green]Location Name[/] (or leave blank):",
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

            return filterOptions;
        }
    }

    public void DisplayShiftsTable(IEnumerable<Shift> shifts)
    {
        if (shifts is null || !shifts.Any())
        {
            AnsiConsole.MarkupLine("[red]No shifts found.[/]");
            return;
        }

        Table table = new();
        table.AddColumn("Shift ID");
        table.AddColumn("Worker");
        table.AddColumn("Location");
        table.AddColumn("Start Time");
        table.AddColumn("End Time");
        table.AddColumn("Duration");

        foreach (var shift in shifts)
        {
            var duration = shift.EndTime - shift.StartTime;
            table.AddRow(
                shift.ShiftId.ToString(),
                shift.Worker?.Name ?? "Unknown Worker",
                shift.Location?.Name ?? "Unknown Location",
                shift.StartTime.ToString("dd/MM/yyyy HH:mm"),
                shift.EndTime.ToString("dd/MM/yyyy HH:mm"),
                $"{duration.Hours:D2}:{duration.Minutes:D2}"
            );
        }

        AnsiConsole.Write(table);
    }

    public int GetShiftByIdUi()
    {
        try
        {
            // Fetch all shifts synchronously (following existing pattern)
            var shiftResponse = shiftService.GetAllShifts(new ShiftFilterOptions()).GetAwaiter().GetResult();
            
            if (shiftResponse.Data == null || shiftResponse.Data.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No shifts available. Please create shifts first.[/]");
                throw new InvalidOperationException("No shifts available for selection.");
            }

            // Create selection choices with shift display information
            var shiftChoices = shiftResponse.Data
                .Select(shift => new { 
                    shift.ShiftId, 
                    Display = $"ID: {shift.ShiftId} | {shift.Worker?.Name ?? "Unknown Worker"} - {shift.Location?.Name ?? "Unknown Location"} | {shift.StartTime:dd/MM/yyyy HH:mm} to {shift.EndTime:HH:mm}" 
                })
                .ToList();

            // Show selection prompt
            var selectedDisplay = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select a shift:[/]")
                    .AddChoices(shiftChoices.Select(choice => choice.Display))
            );

            // Find the selected shift's ID
            var selectedShift = shiftChoices.First(choice => choice.Display == selectedDisplay);
            
            AnsiConsole.MarkupLine($"[green]✓ Selected shift: {selectedDisplay}[/]");
            return selectedShift.ShiftId;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error loading shifts: {ex.Message}[/]");
            throw;
        }
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
            existingWorker.Name ?? string.Empty
        );
        var email = AnsiConsole.Ask<string>(
            "Enter [green]Email[/] (leave blank to keep current):",
            existingWorker.Email ?? string.Empty
        );
        var phoneNumber = AnsiConsole.Ask<string>(
            "Enter [green]Phone Number[/] (leave blank to keep current):",
            existingWorker.PhoneNumber ?? string.Empty
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
            Town = null,
            County = null,
            PostCode = null,
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
            filterLocationOptions.Town = AnsiConsole.Ask<string?>(
                "Enter [green]Town[/] (or leave blank):",
                defaultValue: null
            );
            filterLocationOptions.County = AnsiConsole.Ask<string?>(
                "Enter [green]County[/] (or leave blank):",
                defaultValue: null
            );
            filterLocationOptions.PostCode = AnsiConsole.Ask<string?>(
                "Enter [green]postcode[/] (or leave blank):",
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
        var town = AnsiConsole.Ask<string>("Enter [green]Town[/]:");
        var county = AnsiConsole.Ask<string>("Enter [green]County[/]:");
        var postcode = AnsiConsole.Ask<string>("Enter [green]Postcode[/]:");
        var country = AnsiConsole.Ask<string>("Enter [green]Country[/]:");
        var createdLocation = new Location
        {
            Name = name,
            Address = address,
            Town = town,
            County = county,
            PostCode = postcode,
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
        table.AddColumn("Town");
        table.AddColumn("County");
        table.AddColumn("postcode");
        table.AddColumn("Country");
        // Convert IEnumerable to List for easier indexing
        List<Location> locationsList = [.. locationResponse];

        for (int i = 0; i < locationsList.Count; i++)
        {
            var location = locationsList[i];
            if (location != null)
            {
                table.AddRow(
                    (i + 1).ToString(),
                    location.Name,
                    location.Address,
                    location.Town,
                    location.County,
                    location.PostCode,
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
            existingLocation.Town ?? string.Empty
        );

        var stateOrCounty = AnsiConsole.Ask<string>(
            "Enter [green]County[/] (leave blank to keep current):",
            existingLocation.County ?? string.Empty
        );

        var zipOrPostCode = AnsiConsole.Ask<string>(
            "Enter [green]postcode[/] (leave blank to keep current):",
            existingLocation.PostCode ?? string.Empty
        );

        var country = AnsiConsole.Ask<string>(
            "Enter [green]Country[/] (leave blank to keep current):",
            existingLocation.Country ?? string.Empty
        );
        var updatedLocation = new Location
        {
            Name = name,
            Address = address,
            Town = townOrCity,
            County = stateOrCounty,
            PostCode = zipOrPostCode,
            Country = country,
        };
        return updatedLocation;
    }
}
