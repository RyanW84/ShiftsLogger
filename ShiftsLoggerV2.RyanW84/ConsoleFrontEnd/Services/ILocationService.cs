using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services;

public interface ILocationService
{
    Task<ApiResponseDto<List<Location>>> GetAllLocationsAsync();
    Task<ApiResponseDto<Location?>> GetLocationByIdAsync(int id);
    Task<ApiResponseDto<Location>> CreateLocationAsync(Location location);
    Task<ApiResponseDto<Location?>> UpdateLocationAsync(int id, Location updatedLocation);
    Task<ApiResponseDto<string?>> DeleteLocationAsync(int id);
}
