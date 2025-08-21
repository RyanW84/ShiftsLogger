using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services.Validation;

public static class ShiftValidation
{
    public static List<string> Validate(ShiftApiRequestDto dto)
    {
        var errors = new List<string>();
        if (dto.WorkerId <= 0)
            errors.Add("WorkerId must be greater than zero.");
        if (dto.LocationId <= 0)
            errors.Add("LocationId must be greater than zero.");
        if (dto.StartTime >= dto.EndTime)
            errors.Add("Start time must be before end time.");
        if (dto.StartTime < DateTimeOffset.Now.AddYears(-1) || dto.StartTime > DateTimeOffset.Now.AddYears(1))
            errors.Add("Start time is out of allowed range.");
        if (dto.EndTime < DateTimeOffset.Now.AddYears(-1) || dto.EndTime > DateTimeOffset.Now.AddYears(1))
            errors.Add("End time is out of allowed range.");

        // Additional business rules:
        if ((dto.EndTime - dto.StartTime).TotalMinutes < 15)
            errors.Add("Shift duration must be at least 15 minutes.");
        if ((dto.EndTime - dto.StartTime).TotalHours > 16)
            errors.Add("Shift duration cannot exceed 16 hours.");
        if (dto.StartTime.Date != dto.EndTime.Date)
            errors.Add("Shift must start and end on the same day.");
        if (dto.StartTime < DateTimeOffset.Now.AddMinutes(-5))
            errors.Add("Shift cannot start in the past (with more than 5 minutes tolerance).");
        if (dto.StartTime.DayOfWeek == DayOfWeek.Sunday)
            errors.Add("Shifts cannot start on Sundays.");
        // Example: restrict shifts to business hours (6am-10pm)
        if (dto.StartTime.Hour < 6 || dto.EndTime.Hour > 22)
            errors.Add("Shifts must be within business hours (6am-10pm).");

        // Add more rules as needed for your business logic
        return errors;
    }
}
