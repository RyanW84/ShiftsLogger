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
            AnsiConsole.MarkupLine($"[red]ERROR: {Markup.Escape(message)}[/]");
        }
    }

    public void DisplaySuccess(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[green]SUCCESS: {Markup.Escape(message)}[/]");
        }
    }

    public void DisplayInfo(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[cyan]{Markup.Escape(message)}[/]");
        }
    }

    public void DisplayWarning(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(message)}[/]");
        }
    }

    public void DisplayText(string text)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine(Markup.Escape(text));
        }
    }

    public void DisplayPrompt(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.Markup($"[bold cyan]{Markup.Escape(message)}[/] ");
        }
    }

    public string GetInput()
    {
        lock (_consoleLock)
        {
            return Console.ReadLine() ?? string.Empty;
        }
    }

    public void WaitForKeyPress()
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
    }

    public void DisplayMenu(string title, string[] options)
    {
        lock (_consoleLock)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[bold cyan]{title}[/]").RuleStyle("cyan").LeftJustified());
            AnsiConsole.WriteLine();

            for (int i = 0; i < options.Length; i++)
            {
                AnsiConsole.MarkupLine($"[green]{i + 1}.[/] {Markup.Escape(options[i])}");
            }
            AnsiConsole.WriteLine();
        }
    }

    public void DisplayTable<T>(IEnumerable<T> data, string title)
    {
        lock (_consoleLock)
        {
            var table = new Table();
            table.Title = new TableTitle($"[bold yellow]{title}[/]");
            table.Border = TableBorder.Rounded;

            if (!data.Any())
            {
                AnsiConsole.MarkupLine($"[yellow]No data available for {title}[/]");
                return;
            }

            // Get properties using reflection
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            // Add columns
            foreach (var prop in properties)
            {
                table.AddColumn(new TableColumn($"[bold]{prop.Name}[/]").Centered());
            }

            // Add rows
            foreach (var item in data)
            {
                var values = properties.Select(prop => 
                    Markup.Escape(prop.GetValue(item)?.ToString() ?? "N/A")).ToArray();
                table.AddRow(values);
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
    }

    public void DisplaySeparator()
    {
        lock (_consoleLock)
        {
            AnsiConsole.Write(new Rule().RuleStyle("dim"));
        }
    }

    public void DisplaySystemInfo()
    {
        lock (_consoleLock)
        {
            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.Title = new TableTitle("[bold yellow]System Information[/]");

            table.AddColumn(new TableColumn("[bold]Property[/]"));
            table.AddColumn(new TableColumn("[bold]Value[/]"));

            table.AddRow("Operating System", Environment.OSVersion.ToString());
            table.AddRow("Machine Name", Environment.MachineName);
            table.AddRow("User Name", Environment.UserName);
            table.AddRow(".NET Version", Environment.Version.ToString());
            table.AddRow("Working Directory", Environment.CurrentDirectory);

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
    }
}
