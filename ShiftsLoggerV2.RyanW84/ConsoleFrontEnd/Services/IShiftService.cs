using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.Services;

public interface IShiftService
{
    // This interface defines the contract for shift-related operations, including retrieving, creating, updating, and deleting shifts.

    // It uses asynchronous methods to handle operations that may involve I/O or network communication.
    public Task<ApiResponseDto<List<Shift>>> GetAllShifts(ShiftFilterOptions shiftOptions);
    public Task<ApiResponseDto<Shift>> GetShiftById(int id);
    public Task<ApiResponseDto<Shift>> CreateShift(Shift shift);
    public Task<ApiResponseDto<Shift>> UpdateShift(int id, Shift updatedShift);
    public Task<ApiResponseDto<string>> DeleteShift(int id);
}
