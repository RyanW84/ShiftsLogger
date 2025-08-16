using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd;

public interface IShiftUi
{
    Shift CreateShiftUi(int workerId);
    Shift UpdateShiftUi(Shift existingShift);
    ShiftFilterOptions FilterShiftsUi();
    void DisplayShiftsTable(IEnumerable<Shift> shifts);
    int GetShiftByIdUi();
}
