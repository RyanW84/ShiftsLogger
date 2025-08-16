using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.MenuSystem;

public interface IShiftUi
{
    Shift CreateShift(int workerId);
    Shift UpdateShift(Shift existingShift);
    ShiftFilterOptions FilterShifts();
    void DisplayShiftsTable(IEnumerable<Shift> shifts);
    int GetShiftById();
}
