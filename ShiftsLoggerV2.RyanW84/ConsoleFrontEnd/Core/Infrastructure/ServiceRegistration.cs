using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Core.Infrastructure;
using ConsoleFrontEnd.MenuSystem;
using ConsoleFrontEnd.MenuSystem.Menus;
using ConsoleFrontEnd.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFrontEnd.Core.Infrastructure;

/// <summary>
/// Service registration extension following Dependency Inversion Principle
/// Registers all application services with proper lifetimes
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
        // Core application services
        services.AddSingleton<IApplication, ConsoleApplication>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IMenuFactory, MenuFactory>();

        // Console services
        services.AddSingleton<IConsoleDisplayService, SpectreConsoleDisplayService>();
        services.AddSingleton<IConsoleInputService, SpectreConsoleInputService>();

        // HTTP Client configuration for API services
        services.AddHttpClient("ShiftsLoggerApi", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5181/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Business services - unified approach with fallback capabilities
        services.AddScoped<IShiftService, ConsoleFrontEnd.Services.ShiftService>();
        services.AddScoped<ILocationService, ConsoleFrontEnd.Services.LocationService>();
        services.AddScoped<IWorkerService, ConsoleFrontEnd.Services.WorkerService>();

        // Menu implementations
        services.AddScoped<MainMenuV2>();
        services.AddScoped<ShiftMenuV2>();
    services.AddScoped<IShiftUi, ConsoleFrontEnd.MenuSystem.ShiftUI>();
        services.AddScoped<LocationMenuV2>();
        services.AddScoped<WorkerMenuV2>();

        return services;
    }
}
