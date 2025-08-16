using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.Interfaces.Services;

public interface IShiftService
{
    Task<ApiResponseDto<List<Shift>>> GetAllShifts(ShiftFilterOptions options);
    Task<ApiResponseDto<Shift>> GetShiftById(int id);
    Task<ApiResponseDto<Shift>> CreateShift(Shift shift);
    Task<ApiResponseDto<Shift>> UpdateShift(int id, Shift shift);
    Task<ApiResponseDto<string>> DeleteShift(int id);
}