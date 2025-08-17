using ConsoleFrontEnd.MenuSystem;

namespace ConsoleFrontEnd.Interfaces;

public interface IUserInterfaceFacade
{
    MenuSystem.IShiftUi Shifts { get; }
    MenuSystem.IWorkerUi Workers { get; }
    MenuSystem.ILocationUi Locations { get; }
    IDisplayService Display { get; }
}