using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Core.Infrastructure;
using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.MenuSystem;
using ConsoleFrontEnd.MenuSystem.Menus;
using ConsoleFrontEnd.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFrontEnd.Extensions;

/// <summary>
///     Extension methods for service registration following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Register all application services
    /// </summary>
    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
        // HTTP Client Factory
        services.AddHttpClient();

        // Console services
        services.AddSingleton<IConsoleDisplayService, SpectreConsoleDisplayService>();
        services.AddSingleton<IConsoleInputService, SpectreConsoleInputService>();

        // Core application services
        services.AddSingleton<IMenuFactory, MenuFactory>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IApplication, ConsoleApplication>();

        // API Services
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IWorkerService, WorkerService>();
        services.AddScoped<ILocationService, LocationService>();

        // UI Services
        services.AddScoped<IShiftUi, ShiftUI>();
        services.AddScoped<IWorkerUi, WorkerUi>();
        services.AddScoped<ILocationUi, LocationUI>();
    // Console display service (Spectre.Console-based)
    services.AddSingleton<ConsoleFrontEnd.Core.Abstractions.IConsoleDisplayService, ConsoleFrontEnd.Core.Infrastructure.SpectreConsoleDisplayService>();
    // Console input service (Spectre.Console-based)
    services.AddSingleton<ConsoleFrontEnd.Core.Abstractions.IConsoleInputService, ConsoleFrontEnd.Core.Infrastructure.SpectreConsoleInputService>();
        services.AddScoped<IMenu, MainMenu>();
        services.AddScoped<IMenu, ShiftMenu>();
        services.AddScoped<IMenu, WorkerMenu>();
        services.AddScoped<IMenu, LocationMenu>();
        
        // UI Services
        services.AddScoped<IShiftUi, ShiftUI>();
        services.AddScoped<IWorkerUi, WorkerUi>();
        services.AddScoped<ILocationUi, LocationUI>();
        
        // Register concrete menu types for MenuFactory
        services.AddScoped<MainMenu>();
        services.AddScoped<ShiftMenu>();
        services.AddScoped<WorkerMenu>();
        services.AddScoped<LocationMenu>();
        
        // Register as IMenu interface for polymorphic usage (if needed elsewhere)
        services.AddScoped<IMenu, MainMenu>();
        services.AddScoped<IMenu, ShiftMenu>();
        services.AddScoped<IMenu, WorkerMenu>();
        services.AddScoped<IMenu, LocationMenu>();

        return services;
    }
}