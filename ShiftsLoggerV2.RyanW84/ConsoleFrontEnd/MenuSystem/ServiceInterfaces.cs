using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.MenuSystem;

public interface IShiftServiceLegacy
{
    Task<ApiResponseDto<List<Shift>>> GetAllShifts(ShiftFilterOptions options);
    Task<ApiResponseDto<Shift>> GetShiftById(int id);
    Task<ApiResponseDto<Shift>> CreateShift(Shift shift);
    Task<ApiResponseDto<Shift>> UpdateShift(int id, Shift shift);
    Task<ApiResponseDto<bool>> DeleteShift(int id);
}

public interface ILocationServiceLegacy
{
    Task<ApiResponseDto<List<Location>>> GetAllLocations(LocationFilterOptions options);
    Task<ApiResponseDto<Location>> GetLocationById(int id);
    Task<ApiResponseDto<Location>> CreateLocation(Location location);
    Task<ApiResponseDto<Location>> UpdateLocation(int id, Location location);
    Task<ApiResponseDto<bool>> DeleteLocation(int id);
}

public interface IWorkerServiceLegacy
{
    Task<ApiResponseDto<List<Worker>>> GetAllWorkers(WorkerFilterOptions options);
    Task<ApiResponseDto<Worker>> GetWorkerById(int id);
    Task<ApiResponseDto<Worker>> CreateWorker(Worker worker);
    Task<ApiResponseDto<Worker>> UpdateWorker(int id, Worker worker);
    Task<ApiResponseDto<bool>> DeleteWorker(int id);
}