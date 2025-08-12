using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;

namespace ConsoleFrontEnd.MenuSystem;

public static class InputValidator
{
    public static string GetValidatedString(string prompt, bool required = true, int? maxLength = null)
    {
        while (true)
        {
            var input = AnsiConsole.Ask<string>($"[green]{prompt}[/]");
            
            if (required && string.IsNullOrWhiteSpace(input))
            {
                AnsiConsole.MarkupLine("[red]? This field is required.[/]");
                continue;
            }
            
            if (maxLength.HasValue && input.Length > maxLength.Value)
            {
                AnsiConsole.MarkupLine($"[red]? Maximum length is {maxLength.Value} characters.[/]");
                continue;
            }
            
            return input;
        }
    }
    
    public static string GetValidatedEmail(string prompt)
    {
        while (true)
        {
            var email = AnsiConsole.Ask<string>($"[green]{prompt}[/]");
            
            if (string.IsNullOrWhiteSpace(email))
            {
                AnsiConsole.MarkupLine("[red]? Email is required.[/]");
                continue;
            }
            
            if (!new EmailAddressAttribute().IsValid(email))
            {
                AnsiConsole.MarkupLine("[red]? Please enter a valid email address.[/]");
                continue;
            }
            
            return email;
        }
    }
    
    public static string GetValidatedPhoneNumber(string prompt)
    {
        while (true)
        {
            var phone = AnsiConsole.Ask<string>($"[green]{prompt}[/]");
            
            if (string.IsNullOrWhiteSpace(phone))
            {
                AnsiConsole.MarkupLine("[red]? Phone number is required.[/]");
                continue;
            }
            
            if (!new PhoneAttribute().IsValid(phone))
            {
                AnsiConsole.MarkupLine("[red]? Please enter a valid phone number.[/]");
                continue;
            }
            
            return phone;
        }
    }
    
    public static DateTime GetValidatedDateTime(string prompt, DateTime? minDate = null)
    {
        while (true)
        {
            try
            {
                var dateTime = AnsiConsole.Ask<DateTime>($"[green]{prompt}[/] [dim](yyyy-MM-dd HH:mm)[/]");
                
                if (minDate.HasValue && dateTime < minDate.Value)
                {
                    AnsiConsole.MarkupLine($"[red]? Date must be after {minDate.Value:yyyy-MM-dd HH:mm}.[/]");
                    continue;
                }
                
                return dateTime;
            }
            catch (FormatException)
            {
                AnsiConsole.MarkupLine("[red]? Please enter a valid date and time (yyyy-MM-dd HH:mm).[/]");
            }
        }
    }
    
    public static int GetValidatedInteger(string prompt, int? min = null, int? max = null)
    {
        while (true)
        {
            try
            {
                var value = AnsiConsole.Ask<int>($"[green]{prompt}[/]");
                
                if (min.HasValue && value < min.Value)
                {
                    AnsiConsole.MarkupLine($"[red]? Value must be at least {min.Value}.[/]");
                    continue;
                }
                
                if (max.HasValue && value > max.Value)
                {
                    AnsiConsole.MarkupLine($"[red]? Value must not exceed {max.Value}.[/]");
                    continue;
                }
                
                return value;
            }
            catch (FormatException)
            {
                AnsiConsole.MarkupLine("[red]? Please enter a valid number.[/]");
            }
        }
    }
}