using ConsoleFrontEnd.Interfaces;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.MenuSystem;

public class ShiftUI : IShiftUi
{
    private readonly UserInterface _userInterface;

    public ShiftUI(UserInterface userInterface)
    {
        _userInterface = userInterface;
    }

    public Shift CreateShift(int workerId) => _userInterface.CreateShiftUi(workerId);
    public Shift UpdateShift(Shift existingShift) => _userInterface.UpdateShiftUi(existingShift);
    public ShiftFilterOptions GetShiftFilterOptions() => _userInterface.FilterShiftsUi();
    public void DisplayShifts(IEnumerable<Shift> shifts) => _userInterface.DisplayShiftsTable(shifts);
    public int SelectShift() => _userInterface.GetShiftByIdUi();
}