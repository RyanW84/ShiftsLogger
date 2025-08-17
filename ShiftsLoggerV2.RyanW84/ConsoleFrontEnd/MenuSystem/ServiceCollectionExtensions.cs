using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Core.Infrastructure;
using ConsoleFrontEnd.MenuSystem;
using ConsoleFrontEnd.Services;
using ConsoleFrontEnd.Services.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFrontEnd.Extensions;

/// <summary>
/// Extension methods for service registration following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all application services
    /// </summary>
    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
        // HTTP Client Factory
        services.AddHttpClient();
        
        // Core application services
        services.AddSingleton<IApplication, ConsoleApplication>();
        
        // API Services
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IWorkerService, WorkerService>();
        services.AddScoped<ILocationService, UnifiedLocationService>();
        
        // UI Services
        services.AddScoped<IShiftUi, ShiftUI>();
        services.AddScoped<IWorkerUi, WorkerUI>();
        services.AddScoped<ILocationUi, LocationUI>();
        
        // Helper services
        services.AddScoped<HttpResponseHelper>();
        
        return services;
    }
}
