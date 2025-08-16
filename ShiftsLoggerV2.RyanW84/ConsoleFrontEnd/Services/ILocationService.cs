using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.Services;

public interface ILocationService
{
    public Task<ApiResponseDto<List<Location>>> GetAllLocations(
        LocationFilterOptions locationOptions
    );

    public Task<ApiResponseDto<Location>> GetLocationById(int id);
    public Task<ApiResponseDto<Location>> CreateLocation(Location createdLocation);
    public Task<ApiResponseDto<Location?>> UpdateLocation(int id, Location updatedLocation);
    public Task<ApiResponseDto<string?>> DeleteLocation(int id);
}