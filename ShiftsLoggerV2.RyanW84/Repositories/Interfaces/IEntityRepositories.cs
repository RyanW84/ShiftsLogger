using ShiftsLoggerV2.RyanW84.Core.Interfaces;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;

namespace ShiftsLoggerV2.RyanW84.Repositories.Interfaces;

/// <summary>
/// Repository interface for Shift operations
/// </summary>
public interface IShiftRepository : IRepository<Shift, ShiftFilterOptions, ShiftApiRequestDto, ShiftApiRequestDto>
{
}

/// <summary>
/// Repository interface for Worker operations
/// </summary>
public interface IWorkerRepository : IRepository<Worker, WorkerFilterOptions, WorkerApiRequestDto, WorkerApiRequestDto>
{
}

/// <summary>
/// Repository interface for Location operations
/// </summary>
public interface ILocationRepository : IRepository<Location, LocationFilterOptions, LocationApiRequestDto, LocationApiRequestDto>
{
}
