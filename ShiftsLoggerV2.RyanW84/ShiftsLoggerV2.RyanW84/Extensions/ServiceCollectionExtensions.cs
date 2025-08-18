using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Core.Infrastructure;
using ShiftsLoggerV2.RyanW84.Repositories;
using ShiftsLoggerV2.RyanW84.Repositories.Interfaces;
using ShiftsLoggerV2.RyanW84.Services;

namespace ShiftsLoggerV2.RyanW84.Extensions;

/// <summary>
///     Extension methods for service registration following SOLID principles
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Register all repositories (Data Access Layer)
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IWorkerRepository, WorkerRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();

        return services;
    }

    /// <summary>
    ///     Register all business services (Business Logic Layer)
    /// </summary>
    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<ShiftValidation>();
        services.AddScoped<WorkerValidation>();
        services.AddScoped<LocationValidation>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IWorkerService, WorkerService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IApplication, ConsoleApplication>();


        return services;
    }


    /// <summary>
    ///     Register all application services following SOLID principles
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services
            .AddRepositories()
            .AddBusinessServices();


        return services;
    }
}