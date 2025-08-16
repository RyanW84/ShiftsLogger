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

    public Shift CreateShiftUi(int workerId) => _userInterface.CreateShiftUi(workerId);
    public Shift UpdateShiftUi(Shift existingShift) => _userInterface.UpdateShiftUi(existingShift);
    public ShiftFilterOptions FilterShiftsUi() => _userInterface.FilterShiftsUi();
    public void DisplayShiftsTable(IEnumerable<Shift> shifts) => _userInterface.DisplayShiftsTable(shifts);
    public int GetShiftByIdUi() => _userInterface.GetShiftByIdUi();