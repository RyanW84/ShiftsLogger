using ConsoleFrontEnd.Core.Abstractions;
using Spectre.Console;
using System.Reflection;

namespace ConsoleFrontEnd.Core.Infrastructure;

/// <summary>
/// Console display service implementation using Spectre.Console
/// Following Single Responsibility Principle - handles only display operations
/// </summary>
public class SpectreConsoleDisplayService : IConsoleDisplayService
{
    private static readonly object _consoleLock = new();

    public void Clear()
    {
        lock (_consoleLock)
        {
            AnsiConsole.Clear();
        }
    }

    public void DisplayHeader(string title, string color = "yellow")
    {
        lock (_consoleLock)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold]{title}[/]").RuleStyle(color).Centered());
            AnsiConsole.WriteLine();
        }
    }

    public void DisplayError(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[red]❌ {Markup.Escape(message)}[/]");
        }
    }

    public void DisplaySuccess(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[green]✅ {Markup.Escape(message)}[/]");
        }
    }

    public void DisplayInfo(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[blue]ℹ️ {Markup.Escape(message)}[/]");
        }
    }

    public void DisplayWarning(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[yellow]⚠️ {Markup.Escape(message)}[/]");
        }
    }

    public void DisplayTable<T>(IEnumerable<T> data, string? title = null)
    {
        lock (_consoleLock)
        {
            var table = new Table();
            
            if (!string.IsNullOrEmpty(title))
            {
                table.Title = new TableTitle(title);
            }

            table.Border = TableBorder.Rounded;
            table.BorderColor(Color.Blue);

            if (data?.Any() == true)
            {
                // Get properties using reflection
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                
                // Add columns
                foreach (var prop in properties)
                {
                    table.AddColumn($"[bold]{prop.Name}[/]");
                }

                // Add rows
                foreach (var item in data)
                {
                    var values = properties.Select(prop => 
                        Markup.Escape(prop.GetValue(item)?.ToString() ?? "")).ToArray();
                    table.AddRow(values);
                }
            }
            else
            {
                table.AddColumn("[bold]No Data[/]");
                table.AddRow("No records found");
            }

            AnsiConsole.Write(table);
        }
    }

    public void DisplaySystemInfo()
    {
        lock (_consoleLock)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Cyan1);

            table.AddColumn("[bold]Property[/]");
            table.AddColumn("[bold]Value[/]");

            table.AddRow("Operating System", Environment.OSVersion.ToString());
            table.AddRow("Framework Version", Environment.Version.ToString());
            table.AddRow("Machine Name", Environment.MachineName);
            table.AddRow("User Name", Environment.UserName);
            table.AddRow("Current Directory", Environment.CurrentDirectory);
            table.AddRow("Application Version", Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown");

            AnsiConsole.Write(table);
        }
    }
}
