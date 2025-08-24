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
        // Allow shifts up to 24 hours (can span midnight)
        if ((dto.EndTime - dto.StartTime).TotalHours > 24)
            errors.Add("Shift duration cannot exceed 24 hours.");
        // Allow shifts to span multiple calendar days (e.g., overnight shifts)
        // Keep a small tolerance for past-start to prevent accidental past dates
        if (dto.StartTime < DateTimeOffset.Now.AddMinutes(-5))
            errors.Add("Shift cannot start in the past (with more than 5 minutes tolerance).");

        // Add more rules as needed for your business logic
        return errors;
    }
}
