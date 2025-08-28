using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using System.Threading.Tasks;

namespace ConsoleFrontEnd.MenuSystem;

public interface IShiftUi
{
    Task<Shift> CreateShiftUi(int workerId);
    Task<Shift> UpdateShiftUi(Shift existingShift);
    Task<ShiftFilterOptions> FilterShiftsUi();
    void DisplayShiftsTable(IEnumerable<Shift> shifts);
    Task<int> GetShiftByIdUi();
    Task DisplayShiftsWithPaginationAsync(int initialPageNumber = 1, int pageSize = 10);
}
