using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.MenuSystem.Menus;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFrontEnd.Core.Infrastructure;

/// <summary>
/// Menu factory implementation following Dependency Inversion Principle
/// Creates menu instances with proper dependency injection
/// </summary>
public class MenuFactory : IMenuFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MenuFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IMenu CreateMainMenu()
    {
        return _serviceProvider.GetRequiredService<MainMenuV2>();
    }

    public IMenu CreateShiftMenu()
    {
        return _serviceProvider.GetRequiredService<ShiftMenuV2>();
    }

    public IMenu CreateLocationMenu()
    {
        return _serviceProvider.GetRequiredService<LocationMenuV2>();
    }

    public IMenu CreateWorkerMenu()
    {
        return _serviceProvider.GetRequiredService<WorkerMenuV2>();
    }
}
