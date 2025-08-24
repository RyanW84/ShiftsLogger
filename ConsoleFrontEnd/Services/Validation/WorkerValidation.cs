using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ConsoleFrontEnd.Models.Dtos;

namespace ConsoleFrontEnd.Services.Validation;

public static class WorkerValidation
{
    public static List<string> Validate(WorkerApiRequestDto dto)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Name))
            errors.Add("Worker name is required.");
        if (dto.Name.Length < 2)
            errors.Add("Worker name must be at least 2 characters.");
        if (dto.Name.Length > 100)
            errors.Add("Worker name must be less than 100 characters.");

        // Email validation
        if (string.IsNullOrWhiteSpace(dto.Email))
            errors.Add("Email is required.");
        else
        {
            if (!dto.Email.Contains("@") || dto.Email.Length < 5)
                errors.Add("Email must be valid and at least 5 characters.");
            // Stricter email format validation
            var emailPattern = @"^[A-Za-z0-9](?:[A-Za-z0-9._%+-]{0,62}[A-Za-z0-9])?@[A-Za-z0-9](?:[A-Za-z0-9.-]{0,62}[A-Za-z0-9])?\.[A-Za-z]{2,24}$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Email, emailPattern))
                errors.Add("Email format is invalid.");
            // No consecutive dots
            if (dto.Email.Contains(".."))
                errors.Add("Email cannot contain consecutive dots.");
            // No leading/trailing dot or special character in local or domain part
            var parts = dto.Email.Split('@');
            if (parts.Length == 2)
            {
                if (parts[0].StartsWith('.') || parts[0].EndsWith('.') || parts[1].StartsWith('.') || parts[1].EndsWith('.'))
                    errors.Add("Email cannot start or end with a dot in local or domain part.");
                if (parts[0].StartsWith('-') || parts[0].EndsWith('-') || parts[1].StartsWith('-') || parts[1].EndsWith('-'))
                    errors.Add("Email cannot start or end with a hyphen in local or domain part.");
            }
            // TLD check (common TLDs)
            var tld = dto.Email.Substring(dto.Email.LastIndexOf('.') + 1).ToLower();
            var validTlds = new HashSet<string> { "com", "net", "org", "edu", "gov", "co", "uk", "io", "info", "biz", "me", "us", "ca", "de", "fr", "au", "jp", "cn", "in", "br", "ru", "za", "eu", "ch", "nl", "se", "no", "es", "it", "pl", "tv", "xyz", "site", "online", "tech", "store", "app", "pro", "dev", "ai" };
            if (!validTlds.Contains(tld))
                errors.Add($"Email TLD '{tld}' is not recognized as valid.");
        }

        // Phone validation
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            errors.Add("Phone number is required.");
        else
        {
            // Remove spaces for validation
            var phone = dto.PhoneNumber.Replace(" ", "");
            if (phone.Length < 12)
                errors.Add("Phone number must be at least 12 digits including country code.");
            if (!phone.StartsWith("+44"))
                errors.Add("Phone number must start with +44 for UK format.");
            // Only digits after +44
            if (!System.Text.RegularExpressions.Regex.IsMatch(phone.Substring(3), "^\\d+$"))
                errors.Add("Phone number after +44 must contain only digits.");
        }

        // Add more business rules as needed
        return errors;
    }
}
