using ShiftsLoggerV2.RyanW84.Core.Interfaces;
using ShiftsLoggerV2.RyanW84.Dtos;
using ShiftsLoggerV2.RyanW84.Models;
using ShiftsLoggerV2.RyanW84.Models.FilterOptions;

namespace ShiftsLoggerV2.RyanW84.Services.Interfaces;

/// <summary>
/// Business logic service interface for Shift operations
/// </summary>
public interface IShiftBusinessService : IService<Shift, ShiftFilterOptions, ShiftApiRequestDto, ShiftApiRequestDto>
{
    // Add any business-specific methods here
}

/// <summary>
/// Business logic service interface for Worker operations  
/// </summary>
public interface IWorkerBusinessService : IService<Worker, WorkerFilterOptions, WorkerApiRequestDto, WorkerApiRequestDto>
{
    // Add any business-specific methods here
}

/// <summary>
/// Business logic service interface for Location operations
/// </summary>
public interface ILocationBusinessService : IService<Location, LocationFilterOptions, LocationApiRequestDto, LocationApiRequestDto>
{
    // Add any business-specific methods here
}
