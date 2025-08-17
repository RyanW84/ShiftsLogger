using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services;

public interface IShiftService
{
    Task<ApiResponseDto<List<Shift>>> GetAllShiftsAsync();
    Task<ApiResponseDto<Shift?>> GetShiftByIdAsync(int id);
    Task<ApiResponseDto<Shift>> CreateShiftAsync(Shift shift);
    Task<ApiResponseDto<Shift?>> UpdateShiftAsync(int id, Shift updatedShift);
    Task<ApiResponseDto<string?>> DeleteShiftAsync(int id);
    Task<ApiResponseDto<List<Shift>>> GetShiftsByFilterAsync(ConsoleFrontEnd.Models.FilterOptions.ShiftFilterOptions filter);
}
