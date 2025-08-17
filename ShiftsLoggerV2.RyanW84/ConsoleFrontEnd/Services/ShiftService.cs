using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services;

public class ShiftService : IShiftService
{
    public Task<ApiResponseDto<List<Shift>>> GetAllShiftsAsync()
    {
        // Mock implementation using correct property names
        var shifts = new List<Shift>
        {
            new Shift 
            { 
                ShiftId = 1, 
                WorkerId = 1, 
                LocationId = 1, 
                StartTime = DateTimeOffset.Now.AddHours(-8), 
                EndTime = DateTimeOffset.Now 
            },
            new Shift 
            { 
                ShiftId = 2, 
                WorkerId = 2, 
                LocationId = 2, 
                StartTime = DateTimeOffset.Now.AddHours(-6), 
                EndTime = DateTimeOffset.Now.AddHours(-2) 
            }
        };

        return Task.FromResult(new ApiResponseDto<List<Shift>>("Success")
        {
            Data = shifts,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        });
    }

    public Task<ApiResponseDto<Shift?>> GetShiftByIdAsync(int id)
    {
        // Mock implementation using correct property names
        var shift = new Shift 
        { 
            ShiftId = id, 
            WorkerId = 1, 
            LocationId = 1, 
            StartTime = DateTimeOffset.Now.AddHours(-8), 
            EndTime = DateTimeOffset.Now 
        };

        return Task.FromResult(new ApiResponseDto<Shift?>("Success")
        {
            Data = shift,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        });
    }

    public Task<ApiResponseDto<Shift>> CreateShiftAsync(Shift shift)
    {
        // Mock implementation - assign an ID
        shift.ShiftId = new Random().Next(1, 1000);

        return Task.FromResult(new ApiResponseDto<Shift>("Shift created successfully")
        {
            Data = shift,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.Created
        });
    }

    public Task<ApiResponseDto<Shift?>> UpdateShiftAsync(int id, Shift updatedShift)
    {
        // Mock implementation
        updatedShift.ShiftId = id;

        return Task.FromResult(new ApiResponseDto<Shift?>("Shift updated successfully")
        {
            Data = updatedShift,
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        });
    }

    public Task<ApiResponseDto<string?>> DeleteShiftAsync(int id)
    {
        // Mock implementation
        return Task.FromResult(new ApiResponseDto<string?>("Shift deleted successfully")
        {
            Data = $"Deleted shift with ID {id}",
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK
        });
    }
}
