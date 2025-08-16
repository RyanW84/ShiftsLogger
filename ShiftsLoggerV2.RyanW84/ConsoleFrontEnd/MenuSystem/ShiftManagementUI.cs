// Base display service implementation
public class SpectreConsoleDisplayService : IDisplayService
{
    public void DisplaySuccessMessage(string message)
    {
        AnsiConsole.MarkupLine($"[green]{message}[/]");
    }

    public void DisplayErrorMessage(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }

    public void ContinueAndClearScreen()
    {
        Console.WriteLine("\nPress any key to continue");
        Console.ReadKey();
        Console.Clear();
    }
}

// Input service implementation
public class SpectreConsoleInputService : IInputService
{
    public T GetInput<T>(string prompt, T? defaultValue = default)
    {
        return AnsiConsole.Ask<T>(prompt, defaultValue);
    }
    
    public T GetValidatedInput<T>(string prompt, Func<T, bool> validator, string errorMessage, T? defaultValue = default)
    {
        T input;
        do {
            input = AnsiConsole.Ask<T>(prompt, defaultValue);
            if (validator(input)) break;
            AnsiConsole.MarkupLine($"[red]{errorMessage}[/]");
        } while (true);
        return input;
    }
    
    public string GetSelection(string title, IEnumerable<string> choices)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]{title}[/]")
                .AddChoices(choices)
        );
    }
    
    public DateTime GetDateTime(string prompt, DateTime? minValue = null)
    {
        while (true)
        {
            var input = AnsiConsole.Ask<string>($"[green]{prompt}[/] [dim](dd/MM/yyyy HH:mm):[/]");
            if (DateTime.TryParseExact(input, "dd/MM/yyyy HH:mm", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out var parsed))
            {
                if (minValue == null || parsed > minValue)
                {
                    return parsed;
                }
                AnsiConsole.MarkupLine($"[red]Date must be after {minValue:dd/MM/yyyy HH:mm}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Invalid format. Please use dd/MM/yyyy HH:mm.[/]");
            }
        }
    }
}

// Entity-specific UI implementations
public class ShiftUI : IShiftUI
{
    private readonly IDisplayService _displayService;
    private readonly IInputService _inputService;
    private readonly ILocationUI _locationUI;
    private readonly IShiftService _shiftService;
    
    public ShiftUI(
        IDisplayService displayService, 
        IInputService inputService, 
        ILocationUI locationUI, 
        IShiftService shiftService)
    {
        _displayService = displayService;
        _inputService = inputService;
        _locationUI = locationUI;
        _shiftService = shiftService;
    }
    
    public Shift CreateShift(int workerId)
    {
        AnsiConsole.WriteLine("\nPlease enter the following details for the shift:");
        
        var startTime = _inputService.GetDateTime("Enter Start Time:");
        AnsiConsole.MarkupLine($"[dim]Start time set: {startTime:dd/MM/yyyy HH:mm}[/]");
        
        var endTime = _inputService.GetDateTime("Enter End Time:", startTime.AddMinutes(1));
        
        var duration = endTime - startTime;
        AnsiConsole.MarkupLine($"[green]✓ Start: {startTime:dd/MM/yyyy HH:mm}[/]");
        AnsiConsole.MarkupLine($"[green]✓ End: {endTime:dd/MM/yyyy HH:mm}[/]");
        AnsiConsole.MarkupLine($"[green]✓ Duration: {duration.Hours:D2}:{duration.Minutes:D2}[/]");
        
        var locationId = _locationUI.SelectLocation();
        
        return new Shift
        {
            StartTime = startTime,
            EndTime = endTime,
            LocationId = locationId,
            WorkerId = workerId,
        };
    }
    
    // Implement other methods...
}

public class WorkerUI : IWorkerUI
{
    private readonly IDisplayService _displayService;
    private readonly IInputService _inputService;
    
    public WorkerUI(IDisplayService displayService, IInputService inputService)
    {
        _displayService = displayService;
        _inputService = inputService;
    }
    
    public Worker CreateWorker()
    {
        AnsiConsole.WriteLine("\nPlease enter the following details for the worker:");
        
        var name = _inputService.GetInput<string>("Enter [green]Name[/]:");
        var email = _inputService.GetInput<string>("Enter [green]Email[/]:");
        var phoneNumber = _inputService.GetInput<string>("Enter [green]Phone Number[/]:");
        
        return new Worker
        {
            Name = name,
            Email = email,
            PhoneNumber = phoneNumber,
        };
    }
    
    // Implement other methods...
}

public class LocationUI : ILocationUI
{
    private readonly IDisplayService _displayService;
    private readonly IInputService _inputService;
    private readonly ILocationService _locationService;
    
    public LocationUI(
        IDisplayService displayService, 
        IInputService inputService, 
        ILocationService locationService)
    {
        _displayService = displayService;
        _inputService = inputService;
        _locationService = locationService;
    }
    
    public Location CreateLocation()
    {
        AnsiConsole.WriteLine("\nPlease enter the following details for the location:");
        
        var name = _inputService.GetInput<string>("Enter [green]Name[/]:");
        var address = _inputService.GetInput<string>("Enter [green]Address[/]:");
        var town = _inputService.GetInput<string>("Enter [green]Town[/]:");
        var county = _inputService.GetInput<string>("Enter [green]County[/]:");
        var postcode = _inputService.GetInput<string>("Enter [green]Postcode[/]:");
        var country = _inputService.GetInput<string>("Enter [green]Country[/]:");
        
        return new Location
        {
            Name = name,
            Address = address,
            Town = town,
            County = county,
            PostCode = postcode,
            Country = country,
        };
    }
    
    // Implement other methods...
}
