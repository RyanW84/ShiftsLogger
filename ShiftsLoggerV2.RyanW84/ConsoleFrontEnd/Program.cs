using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.MenuSystem;
using ConsoleFrontEnd.Services;
// UI interface aliases - use fully qualified names to avoid ambiguity

// Service interface aliases
using IShiftService = ConsoleFrontEnd.Services.IShiftService;
using ILocationService = ConsoleFrontEnd.Services.ILocationService;
using ILocationUi = ConsoleFrontEnd.MenuSystem.ILocationUi;
using ConsoleFrontEnd.MenuSystem;
using ConsoleFrontEnd.Services;
using ConsoleFrontEnd.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFrontEnd;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Create service collection
        var services = new ServiceCollection();

        // Register services
        services.AddSingleton<IDisplayService, SpectreConsoleDisplayService>();
        services.AddSingleton<IInputService, SpectreConsoleInputService>();

        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IWorkerService, WorkerService>();

        // Register UI implementations
        services.AddScoped<IShiftUi, ShiftUI>();
        services.AddScoped<IWorkerUi, WorkerUI>();
        services.AddScoped<ILocationUi, LocationUI>();

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();

        // Create the main menu using the service provider
        var mainMenu = new MainMenu(serviceProvider);
        await mainMenu.RunAsync();
        }
        services.AddScoped<IWorkerUi, WorkerUi>();
        services.AddScoped<ILocationUi, LocationUI>();

// Facade to simplify UI access for controllers
        services.AddScoped<IUserInterfaceFacade, UserInterfaceFacade>();

        await MainMenu.DisplayMainMenu();
    }
}