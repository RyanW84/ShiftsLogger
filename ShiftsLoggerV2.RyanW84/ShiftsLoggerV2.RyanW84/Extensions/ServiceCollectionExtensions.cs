using ShiftsLoggerV2.RyanW84.Repositories;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Services;
using ShiftsLoggerV2.RyanW84.Services.Interfaces;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Core.Interfaces;

namespace ShiftsLoggerV2.RyanW84.Extensions;

/// <summary>
/// Extension methods for service registration following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all repositories (Data Access Layer)
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IWorkerRepository, WorkerRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        
        return services;
    }

    /// <summary>
    /// Register all business services (Business Logic Layer)
    /// </summary>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<ShiftBusinessService>();
        services.AddScoped<WorkerBusinessService>();
        services.AddScoped<LocationBusinessService>();
        
        return services;
    }

    /// <summary>
    /// Register legacy services for backward compatibility
    /// </summary>
    public static IServiceCollection AddLegacyServices(this IServiceCollection services)
    {
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IWorkerService, WorkerService>();
        services.AddScoped<ILocationService, LocationService>();
        
        return services;
    }

    /// <summary>
    /// Register all application services following SOLID principles
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services
            .AddRepositories()
            .AddBusinessServices()
            .AddLegacyServices(); // Keep for backward compatibility
            
        return services;
    }
}
