public interface IShiftService
{
    Task<ApiResponse<List<Shift>>> GetAllShifts(ShiftFilterOptions options);
    Task<ApiResponse<Shift>> GetShiftById(int id);
    Task<ApiResponse<Shift>> CreateShift(Shift shift);
    Task<ApiResponse<Shift>> UpdateShift(int id, Shift shift);
    Task<ApiResponse<bool>> DeleteShift(int id);
}

public interface ILocationService
{
    Task<ApiResponse<List<Location>>> GetAllLocations(LocationFilterOptions options);
    Task<ApiResponse<Location>> GetLocationById(int id);
    Task<ApiResponse<Location>> CreateLocation(Location location);
    Task<ApiResponse<Location>> UpdateLocation(int id, Location location);
    Task<ApiResponse<bool>> DeleteLocation(int id);
}

public interface IWorkerService
{
    Task<ApiResponse<List<Worker>>> GetAllWorkers(WorkerFilterOptions options);
    Task<ApiResponse<Worker>> GetWorkerById(int id);
    Task<ApiResponse<Worker>> CreateWorker(Worker worker);
    Task<ApiResponse<Worker>> UpdateWorker(int id, Worker worker);
    Task<ApiResponse<bool>> DeleteWorker(int id);
}
